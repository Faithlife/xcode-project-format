using System.Globalization;

namespace Faithlife.XcodeProjectFormat.Serialization;

internal readonly record struct XCJsonPathComponent
{
	public static XCJsonPathComponent ForKey(string key) => new(key, -1);

	public static XCJsonPathComponent ForIndex(int index) => new(null, index);

	public string? Key { get; }

	public int Index { get; }

	public override string ToString() => Key is not null ? "/" + Key : "[" + Index.ToString(CultureInfo.InvariantCulture) + "]";

	private XCJsonPathComponent(string? key, int index)
	{
		Key = key;
		Index = index;
	}
}

/// <summary>
/// An absolute path from the root of a JSON document to a value within it.
/// </summary>
internal sealed class XCJsonPath : IEquatable<XCJsonPath>
{
	public static readonly XCJsonPath Root = new(null, default);

	public XCJsonPath? Parent { get; }

	public XCJsonPathComponent? Component => Parent is null ? null : m_component;

	public bool IsRoot => Parent is null;

	public IReadOnlyList<XCJsonPathComponent> Components
	{
		get
		{
			var components = new List<XCJsonPathComponent>();
			for (var path = this; path.Parent is not null; path = path.Parent)
				components.Add(path.m_component);
			components.Reverse();
			return components;
		}
	}

	public XCJsonPath Append(XCJsonPathComponent component) => new(this, component);

	public XCJsonPath Append(string key) => Append(XCJsonPathComponent.ForKey(key));

	public XCJsonPath Append(int index) => Append(XCJsonPathComponent.ForIndex(index));

	public bool Equals(XCJsonPath? other)
	{
		var left = this;
		var right = other;
		while (true)
		{
			if (ReferenceEquals(left, right))
				return true;
			if (left is null || right is null || left.m_hashCode != right.m_hashCode || left.Parent is null || right.Parent is null || left.m_component != right.m_component)
				return false;
			left = left.Parent;
			right = right.Parent;
		}
	}

	public override bool Equals(object? obj) => obj is XCJsonPath other && Equals(other);

	public override int GetHashCode() => m_hashCode;

	public override string ToString() => string.Concat(Components.Select(x => x.ToString()));

	private XCJsonPath(XCJsonPath? parent, XCJsonPathComponent component)
	{
		Parent = parent;
		m_component = component;
		m_hashCode = parent is null ? 0 : HashCode.Combine(parent.m_hashCode, component);
	}

	private readonly XCJsonPathComponent m_component;
	private readonly int m_hashCode;
}
