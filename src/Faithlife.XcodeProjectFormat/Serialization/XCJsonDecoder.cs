using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace Faithlife.XcodeProjectFormat.Serialization;

/// <summary>
/// Decodes values from <see cref="XCJsonValue"/>.
/// </summary>
internal sealed class XCJsonDecoder
{
	public static T DecodeValue<T>(XCJsonValue value) => new XCJsonDecoder(value).Decode<T>();

	public static T DecodeText<T>(string text) => DecodeValue<T>(XCJsonValue.Parse(text));

	public XCJsonValueType CurrentNodeType => m_currentValue.Type;

	public PrimitiveContainer OpenPrimitiveContainer() => new(this);

	public OrdinalContainer OpenOrdinalContainer() => new(this);

	public KeyedContainer OpenKeyedContainer() => new(this);

	public bool DecodePrimitiveBoolean() => OpenPrimitiveContainer().DecodeBoolean();

	public long DecodePrimitiveInteger() => OpenPrimitiveContainer().DecodeInteger();

	public double DecodePrimitiveDouble() => OpenPrimitiveContainer().DecodeDouble();

	public string DecodePrimitiveString() => OpenPrimitiveContainer().DecodeString();

	/// <summary>
	/// Decodes the current value as the specified type.
	/// </summary>
	public T Decode<T>() => (T) DecodeCurrent(typeof(T))!;

	private object? DecodeCurrent(Type type) => GetDecoder(type)(this);

	private object? DecodeChild(XCJsonValue value, XCJsonPathComponent component, Type type)
	{
		var originalPath = m_currentPath;
		var originalValue = m_currentValue;
		var path = originalPath.Append(component);
		m_currentPath = path;
		m_currentValue = value;
		try
		{
			return DecodeCurrent(type);
		}
		catch (XcodeProjectFormatException exception) when (exception.CodingPath is null)
		{
			throw exception.WithCodingPath(path.ToString());
		}
		finally
		{
			m_currentPath = originalPath;
			m_currentValue = originalValue;
		}
	}

	private XcodeProjectFormatException CreateTypeMismatchException(XCJsonValueType expectedType) =>
		new($"Expected an instance of {XCJsonValue.GetErrorMessageName(expectedType)} but an instance {m_currentValue.Type.ToString().ToLowerInvariant()} was specified");

	public sealed class PrimitiveContainer
	{
		public bool IsNull => m_value.Type == XCJsonValueType.Null;

		public bool DecodeBoolean() => m_value.Type == XCJsonValueType.Boolean ? m_value.BooleanValue : throw m_decoder.CreateTypeMismatchException(XCJsonValueType.Boolean);

		public long DecodeInteger() => m_value.Type == XCJsonValueType.Integer ? m_value.IntegerValue : throw m_decoder.CreateTypeMismatchException(XCJsonValueType.Integer);

		// integers are accepted, since doubles with integral values are encoded without a decimal point
		public double DecodeDouble() => m_value.Type switch
		{
			XCJsonValueType.Double => m_value.DoubleValue,
			XCJsonValueType.Integer => m_value.IntegerValue,
			_ => throw m_decoder.CreateTypeMismatchException(XCJsonValueType.Double),
		};

		public string DecodeString() => m_value.Type == XCJsonValueType.String ? m_value.StringValue : throw m_decoder.CreateTypeMismatchException(XCJsonValueType.String);

		internal PrimitiveContainer(XCJsonDecoder decoder)
		{
			m_decoder = decoder;
			m_value = decoder.m_currentValue;
		}

		private readonly XCJsonDecoder m_decoder;
		private readonly XCJsonValue m_value;
	}

	public sealed class OrdinalContainer
	{
		public int Count => m_elements.Count;

		public T Decode<T>(int index) => (T) m_decoder.DecodeChild(m_elements[index], XCJsonPathComponent.ForIndex(index), typeof(T))!;

		internal object? Decode(int index, Type type) => m_decoder.DecodeChild(m_elements[index], XCJsonPathComponent.ForIndex(index), type);

		internal OrdinalContainer(XCJsonDecoder decoder)
		{
			if (decoder.m_currentValue.Type != XCJsonValueType.Array)
				throw decoder.CreateTypeMismatchException(XCJsonValueType.Array);
			m_decoder = decoder;
			m_elements = decoder.m_currentValue.ArrayEntries.Where(x => x.IsValue).Select(x => x.Value!).ToList();
		}

		private readonly XCJsonDecoder m_decoder;
		private readonly List<XCJsonValue> m_elements;
	}

	public sealed class KeyedContainer
	{
		public int Count => m_fields.Count;

		public IEnumerable<string> Keys => m_fields.Keys;

		public bool Contains(string key) => m_fields.ContainsKey(key);

		/// <summary>
		/// Decodes the required value for the specified key.
		/// </summary>
		public T Decode<T>(string key) => (T) Decode(key, typeof(T))!;

		/// <summary>
		/// Decodes the value for the specified key, or returns the default value if the key is missing.
		/// </summary>
		public T Decode<T>(string key, T defaultValue) => m_fields.TryGetValue(key, out var value) ? (T) m_decoder.DecodeChild(value, XCJsonPathComponent.ForKey(key), typeof(T))! : defaultValue;

		/// <summary>
		/// Decodes the value for the specified key, or returns null if the key is missing or the value is null.
		/// </summary>
		public T? DecodeOptional<T>(string key)
			where T : class => m_fields.TryGetValue(key, out var value) && value.Type != XCJsonValueType.Null ? (T) m_decoder.DecodeChild(value, XCJsonPathComponent.ForKey(key), typeof(T))! : null;

		/// <summary>
		/// Decodes the value for the specified key, or returns null if the key is missing or the value is null.
		/// </summary>
		public T? DecodeOptionalValue<T>(string key)
			where T : struct => m_fields.TryGetValue(key, out var value) && value.Type != XCJsonValueType.Null ? (T) m_decoder.DecodeChild(value, XCJsonPathComponent.ForKey(key), typeof(T))! : null;

		/// <summary>
		/// Decodes the value for the specified key; returns the default value if the key is missing, or null if the value is null.
		/// </summary>
		public T? DecodeOptionalWithNonNullDefault<T>(string key, T defaultValue)
			where T : class
		{
			if (!m_fields.TryGetValue(key, out var value))
				return defaultValue;
			return value.Type == XCJsonValueType.Null ? null : (T) m_decoder.DecodeChild(value, XCJsonPathComponent.ForKey(key), typeof(T))!;
		}

		/// <summary>
		/// Decodes a type whose properties are stored inline in this container.
		/// </summary>
		public T DecodeInline<T>()
			where T : IXCJsonInlineKeyedDecodable<T> => T.Decode(this);

		internal object? Decode(string key, Type type)
		{
			if (!m_fields.TryGetValue(key, out var value))
				throw new XcodeProjectFormatException($"Missing required value for key {key.SmartQuoted()} at {m_path.ToString().SmartQuoted()}");
			return m_decoder.DecodeChild(value, XCJsonPathComponent.ForKey(key), type);
		}

		internal KeyedContainer(XCJsonDecoder decoder)
		{
			if (decoder.m_currentValue.Type != XCJsonValueType.Object)
				throw decoder.CreateTypeMismatchException(XCJsonValueType.Object);
			m_decoder = decoder;
			m_path = decoder.m_currentPath;
			m_fields = new Dictionary<string, XCJsonValue>(StringComparer.Ordinal);
			foreach (var entry in decoder.m_currentValue.ObjectEntries)
			{
				if (entry.Field is { } field)
					m_fields[field.Key] = field.Value;
			}
		}

		private readonly XCJsonDecoder m_decoder;
		private readonly XCJsonPath m_path;
		private readonly Dictionary<string, XCJsonValue> m_fields;
	}

	private XCJsonDecoder(XCJsonValue root)
	{
		m_currentValue = root;
		m_currentPath = XCJsonPath.Root;
	}

	private static Func<XCJsonDecoder, object?> GetDecoder(Type type) => s_decoders.GetOrAdd(type, CreateDecoder);

	private static Func<XCJsonDecoder, object?> CreateDecoder(Type type)
	{
		if (type == typeof(bool))
			return static x => x.DecodePrimitiveBoolean();
		if (type == typeof(long))
			return static x => x.DecodePrimitiveInteger();
		if (type == typeof(int))
			return static x => checked((int) x.DecodePrimitiveInteger());
		if (type == typeof(double))
			return static x => x.DecodePrimitiveDouble();
		if (type == typeof(float))
			return static x => (float) x.DecodePrimitiveDouble();
		if (type == typeof(string))
			return static x => x.DecodePrimitiveString();
		if (type == typeof(XCJsonValue))
			return static x => x.m_currentValue;
		if (type.IsEnum)
			return x => XCJsonCoding.ParseEnumName(type, x.DecodePrimitiveString());

		if (Nullable.GetUnderlyingType(type) is { } underlyingType)
		{
			var underlyingDecoder = GetDecoder(underlyingType);
			return x => x.CurrentNodeType == XCJsonValueType.Null ? null : underlyingDecoder(x);
		}

		if (ImplementsSelfInterface(type, typeof(IXCJsonDecodable<>)))
			return typeof(XCJsonDecoder).GetMethod(nameof(CreateCustomDecoder), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type).CreateDelegate<Func<XCJsonDecoder, object?>>();

		// a concrete subclass of a polymorphic type is decoded via its base type
		for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
		{
			if (ImplementsSelfInterface(baseType, typeof(IXCJsonDecodable<>)))
			{
				var baseDecoder = GetDecoder(baseType);
				return x => baseDecoder(x) is { } value && type.IsInstanceOfType(value) ? value : throw new XcodeProjectFormatException($"Expected {type.Name} at {x.m_currentPath.ToString().SmartQuoted()}");
			}
		}

		if (type.IsGenericType)
		{
			var definition = type.GetGenericTypeDefinition();
			var arguments = type.GetGenericArguments();
			if (definition == typeof(List<>))
			{
				var elementType = arguments[0];
				return x =>
				{
					var container = x.OpenOrdinalContainer();
					var list = (IList) Activator.CreateInstance(type, container.Count)!;
					for (var index = 0; index < container.Count; index++)
						list.Add(container.Decode(index, elementType));
					return list;
				};
			}

			if (definition == typeof(HashSet<>))
			{
				var elementType = arguments[0];
				var addMethod = type.GetMethod(nameof(HashSet<>.Add))!;
				return x =>
				{
					var container = x.OpenOrdinalContainer();
					var set = Activator.CreateInstance(type)!;
					var count = 0;
					for (var index = 0; index < container.Count; index++)
					{
						if ((bool) addMethod.Invoke(set, [container.Decode(index, elementType)])!)
							count++;
					}
					if (count != container.Count)
						throw new XcodeProjectFormatException("Decoding a set produced duplicate elements");
					return set;
				};
			}

			if (definition == typeof(Dictionary<,>))
			{
				var keyParser = GetKeyParser(arguments[0]);
				var valueType = arguments[1];
				return x =>
				{
					var container = x.OpenKeyedContainer();
					var dictionary = (IDictionary) Activator.CreateInstance(type)!;
					foreach (var key in container.Keys)
						dictionary[keyParser(key)] = container.Decode(key, valueType);
					return dictionary;
				};
			}
		}

		throw new InvalidOperationException($"Type {type} can't be decoded from JSON.");
	}

	private static Func<string, object> GetKeyParser(Type keyType)
	{
		if (keyType == typeof(string))
			return static x => x;
		if (keyType.IsEnum)
			return x => XCJsonCoding.ParseEnumName(keyType, x);

		if (ImplementsSelfInterface(keyType, typeof(IXCJsonStringCodable<>)))
			return typeof(XCJsonDecoder).GetMethod(nameof(CreateKeyParser), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(keyType).CreateDelegate<Func<string, object>>();

		throw new InvalidOperationException($"Type {keyType} can't be decoded from a JSON dictionary key.");
	}

	private static bool ImplementsSelfInterface(Type type, Type interfaceDefinition) =>
		type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceDefinition && x.GetGenericArguments()[0] == type);

	private static object? CreateCustomDecoder<T>(XCJsonDecoder decoder)
		where T : IXCJsonDecodable<T> => T.Decode(decoder);

	private static object CreateKeyParser<T>(string key)
		where T : IXCJsonStringCodable<T> => T.FromEncodableStringRepresentation(key);

	private static readonly ConcurrentDictionary<Type, Func<XCJsonDecoder, object?>> s_decoders = new();

	private XCJsonValue m_currentValue;
	private XCJsonPath m_currentPath;
}
