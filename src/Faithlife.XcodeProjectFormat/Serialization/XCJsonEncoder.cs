using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Faithlife.XcodeProjectFormat.Serialization;

/// <summary>
/// Encodes values as <see cref="XCJsonValue"/>, recording requested printing densities.
/// </summary>
internal sealed class XCJsonEncoder
{
	public static XCJsonValue EncodeToValue(object? value) => new XCJsonEncoder().Encode(value);

	public static string EncodeToText(object? value, XCJsonEncodingOptions? options = null) => Encoding.UTF8.GetString(EncodeToUtf8(value, options));

	public static byte[] EncodeToUtf8(object? value, XCJsonEncodingOptions? options = null)
	{
		var encoder = new XCJsonEncoder();
		var json = encoder.Encode(value);
		return XCJsonPrinter.PrintUtf8(json, options ?? XCJsonEncodingOptions.Default, encoder.m_densities);
	}

	public PrimitiveContainer OpenPrimitiveContainer() => OpenContainer(new PrimitiveContainer(this), null);

	public OrdinalContainer OpenOrdinalContainer(XCJsonPrintingDensity? density = null) => OpenContainer(new OrdinalContainer(this), density);

	public KeyedContainer OpenKeyedContainer(XCJsonPrintingDensity? density = null) => OpenContainer(new KeyedContainer(this), density);

	private TContainer OpenContainer<TContainer>(TContainer container, XCJsonPrintingDensity? density)
		where TContainer : Container
	{
		m_currentContainer = container;
		if (density is { } value)
			m_densities[container.Path] = value;
		return container;
	}

	private XCJsonValue Encode(object? value)
	{
		var originalContainer = m_currentContainer;
		EncodeInCurrentContext(value);
		if (ReferenceEquals(originalContainer, m_currentContainer))
			return XCJsonValue.FromObject();

		var openedContainer = m_currentContainer!;
		if (!ReferenceEquals(openedContainer.ParentContainer, originalContainer))
			throw new InvalidOperationException("Exactly one container must be opened when encoding a value.");
		m_currentContainer = originalContainer;
		return openedContainer.FinishEncoding();
	}

	private void EncodeInCurrentContext(object? value)
	{
		switch (value)
		{
			case null:
				OpenPrimitiveContainer().EncodeNull();
				break;
			case IXCJsonEncodable encodable:
				encodable.Encode(this);
				break;
			case bool boolean:
				OpenPrimitiveContainer().Encode(boolean);
				break;
			case int integer:
				OpenPrimitiveContainer().Encode(integer);
				break;
			case long integer:
				OpenPrimitiveContainer().Encode(integer);
				break;
			case double number:
				OpenPrimitiveContainer().Encode(number);
				break;
			case float number:
				OpenPrimitiveContainer().Encode(number);
				break;
			case string text:
				OpenPrimitiveContainer().Encode(text);
				break;
			case Enum enumValue:
				OpenPrimitiveContainer().Encode(XCJsonCoding.GetEnumName(enumValue));
				break;
			case XCJsonValue json:
				OpenPrimitiveContainer().EncodeValue(json);
				break;
			case IDictionary dictionary:
				XCJsonCoding.EncodeDictionary(this, dictionary, null);
				break;
			case IEnumerable enumerable:
				var container = OpenOrdinalContainer();
				foreach (var element in XCJsonCoding.GetOrderedElements(enumerable))
					container.Encode(element);
				break;
			default:
				throw new ArgumentException($"Type {value.GetType()} can't be encoded as JSON.", nameof(value));
		}
	}

	public abstract class Container
	{
		public void Comment(XCJsonCommentStyle style, string content) => AddComment(new XCJsonComment(style, content));

		private protected Container(XCJsonEncoder encoder)
		{
			Encoder = encoder;
			ParentContainer = encoder.m_currentContainer;
			Path = ParentContainer is null ? XCJsonPath.Root : ParentContainer.Path.Append(ParentContainer.ComponentBeingEncoded ?? throw new InvalidOperationException("A container can only be opened while encoding a value."));
		}

		internal XCJsonEncoder Encoder { get; }

		internal Container? ParentContainer { get; }

		internal XCJsonPath Path { get; }

		internal XCJsonPathComponent? ComponentBeingEncoded { get; set; }

		private protected abstract void AddComment(XCJsonComment comment);

		internal abstract XCJsonValue FinishEncoding();
	}

	public sealed class PrimitiveContainer : Container
	{
		public void EncodeNull() => EncodeValue(XCJsonValue.Null);

		public void Encode(bool value) => EncodeValue(XCJsonValue.FromBoolean(value));

		public void Encode(long value) => EncodeValue(XCJsonValue.FromInteger(value));

		public void Encode(double value) => EncodeValue(XCJsonValue.FromDouble(value));

		public void Encode(string value) => EncodeValue(XCJsonValue.FromString(value));

		internal PrimitiveContainer(XCJsonEncoder encoder)
			: base(encoder)
		{
		}

		internal void EncodeValue(XCJsonValue value)
		{
			if (m_value is not null)
				throw new InvalidOperationException("A primitive container can only encode one value.");
			m_value = value;
		}

		private protected override void AddComment(XCJsonComment comment) => throw new InvalidOperationException("Primitive containers can't contain comments.");

		internal override XCJsonValue FinishEncoding() => m_value ?? throw new InvalidOperationException("No value was encoded.");

		private XCJsonValue? m_value;
	}

	public sealed class OrdinalContainer : Container
	{
		public void Encode(object? value, XCJsonPrintingDensity? density = null)
		{
			var index = m_entries.Count;
			ComponentBeingEncoded = XCJsonPathComponent.ForIndex(index);
			m_entries.Add(Encoder.Encode(value));
			ComponentBeingEncoded = null;
			if (density is { } densityValue)
				Encoder.m_densities[Path.Append(index)] = densityValue;
		}

		internal OrdinalContainer(XCJsonEncoder encoder)
			: base(encoder)
		{
			m_entries = [];
		}

		private protected override void AddComment(XCJsonComment comment) => m_entries.Add(comment);

		internal override XCJsonValue FinishEncoding() => XCJsonValue.FromArray(m_entries);

		private readonly List<XCJsonValueOrComment> m_entries;
	}

	public sealed class KeyedContainer : Container
	{
		/// <summary>
		/// Encodes the value for the specified (spear-case) key, even if it is null or a default value.
		/// </summary>
		public void Encode(string key, object? value, XCJsonPrintingDensity? density = null)
		{
			Debug.Assert(XCJsonCoding.IsSpearCase(key), $"Key {key} is not spear-case.");
			EncodeUnverified(key, value, density);
		}

		/// <summary>
		/// Encodes the value for the specified key (which is not required to be spear-case).
		/// </summary>
		public void EncodeUnverified(string key, object? value, XCJsonPrintingDensity? density = null)
		{
#if DEBUG
			if (!m_encodedKeys.Add(key))
				throw new InvalidOperationException($"Encoded the key {key.SmartQuoted()} multiple times.");
#endif
			ComponentBeingEncoded = XCJsonPathComponent.ForKey(key);
			if (density is { } densityValue)
				Encoder.m_densities[Path.Append(key)] = densityValue;
			m_entries.Add(new XCJsonFieldOrComment(key, Encoder.Encode(value)));
			ComponentBeingEncoded = null;
		}

		/// <summary>
		/// Encodes the value for the specified key unless it is (structurally) equal to the default value.
		/// </summary>
		public void EncodeIfNotDefault<T>(string key, T value, T defaultValue, XCJsonPrintingDensity? density = null)
		{
			if (!XCJsonCoding.AreEqual(value, defaultValue))
				Encode(key, value, density);
		}

		/// <summary>
		/// Encodes the value for the specified key unless it is null.
		/// </summary>
		public void EncodeIfNotNull(string key, object? value, XCJsonPrintingDensity? density = null)
		{
			if (value is not null)
				Encode(key, value, density);
		}

		/// <summary>
		/// Encodes the value for the specified key unless it is null or an empty collection.
		/// </summary>
		public void EncodeIfNotEmpty(string key, IEnumerable? value, XCJsonPrintingDensity? density = null)
		{
			if (!XCJsonCoding.IsNullOrEmpty(value))
				Encode(key, value, density);
		}

		/// <summary>
		/// Encodes the properties of the specified value into this container.
		/// </summary>
		public void EncodeInline(IXCJsonInlineKeyedEncodable? value) => value?.Encode(this);

		internal KeyedContainer(XCJsonEncoder encoder)
			: base(encoder)
		{
			m_entries = [];
		}

		private protected override void AddComment(XCJsonComment comment) => m_entries.Add(comment);

		internal override XCJsonValue FinishEncoding() => XCJsonValue.FromObject(m_entries);

		private readonly List<XCJsonFieldOrComment> m_entries;
#if DEBUG
		private readonly HashSet<string> m_encodedKeys = new(StringComparer.Ordinal);
#endif
	}

	private XCJsonEncoder()
	{
		m_densities = [];
	}

	private readonly Dictionary<XCJsonPath, XCJsonPrintingDensity> m_densities;
	private Container? m_currentContainer;
}
