﻿using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Tests.SampleClasses; 

public class PersonForXml {
    public string Name { get; set; }

    public DateTime StartDate { get; set; }

    public int Age { get; set; }

    public decimal Percent { get; set; }

    public long BigNumber { get; set; }

    public bool IsCool { get; set; }

    public List<Friend> Friends { get; set; }

    public Friend BestFriend { get; set; }

    protected string Ignore { get; set; }

    public string IgnoreProxy => Ignore;

    protected string ReadOnly => null;

    public string ReadOnlyProxy => ReadOnly;

    public FoeList Foes { get; set; }

    public Guid UniqueId { get; set; }

    public Guid EmptyGuid { get; set; }

    public Uri Url { get; set; }

    public Uri UrlPath { get; set; }

    public Order Order { get; set; }

    public Disposition Disposition { get; set; }

    public Band FavoriteBand { get; set; }

    public class Band {
        public string Name { get; set; }
    }
}

public class ValueCollectionForXml {
    public string Value { get; set; }
    public List<ValueForXml> Values { get; set; }
}

public class ValueForXml {
    public DateTime Timestamp { get; set; }
    public string Value { get; set; }
}

public class IncomingInvoice {
    public int ConceptId { get; set; }
}

public class PersonForJson {
    public string Name { get; set; }

    public DateTime StartDate { get; set; }

    public int Age { get; set; }

    public decimal Percent { get; set; }

    public long BigNumber { get; set; }

    public bool IsCool { get; set; }

    public List<Friend> Friends { get; set; }

    public Friend BestFriend { get; set; }

    public Guid Guid { get; set; }

    public Guid EmptyGuid { get; set; }

    public Uri Url { get; set; }

    public Uri UrlPath { get; set; }

    protected string Ignore { get; set; }

    public string IgnoreProxy => Ignore;

    protected string ReadOnly => null;

    public string ReadOnlyProxy => ReadOnly;

    public Dictionary<string, Foe> Foes { get; set; }

    public Order Order { get; set; }

    public Disposition Disposition { get; set; }

    public int PersonId { get; set; }
}

public enum Order { First, Second, Third }

public enum Disposition { Friendly, SoSo, SteerVeryClear }

public class Friend {
    public string Name { get; set; }

    public int Since { get; set; }
}

public class Foe {
    public string Nickname { get; set; }
}

public class FoeList : List<Foe> {
    public string Team { get; set; }
}

public class Birthdate {
    public DateTime Value { get; set; }
}

public class OrderedProperties {
    [SerializeAs(Index = 2)]
    public string Name { get; set; }

    [SerializeAs(Index = 3)]
    public int Age { get; set; }

    [SerializeAs(Index = 1)]
    public DateTime StartDate { get; set; }
}

public class ObjectProperties {
    public object ObjectProperty { get; set; }
}

public class DatabaseCollection : List<Database> { }

public class Database {
    public string Name { get; set; }

    public string InitialCatalog { get; set; }

    public string DataSource { get; set; }
}

public class Generic<T> {
    public T Data { get; set; }
}

public class GenericWithList<T> {
    public List<T> Items { get; set; }
}

public class GuidList {
    public List<Guid> Ids { get; set; }
}

public class DateTimeTestStructure {
    public DateTime DateTime { get; set; }

    public DateTime? NullableDateTimeWithNull { get; set; }

    public DateTime? NullableDateTimeWithValue { get; set; }

    public DateTimeOffset DateTimeOffset { get; set; }

    public DateTimeOffset? NullableDateTimeOffsetWithNull { get; set; }

    public DateTimeOffset? NullableDateTimeOffsetWithValue { get; set; }
}

public class Iso8601DateTimeTestStructure {
    public DateTime DateTimeLocal { get; set; }

    public DateTime DateTimeUtc { get; set; }

    public DateTime DateTimeWithOffset { get; set; }
}

public class NewDateTimeTestStructure {
    public DateTime DateTime { get; set; }

    public DateTime DateTimeNegative { get; set; }
}

public class TimeSpanTestStructure {
    public TimeSpan Tick { get; set; }

    public TimeSpan Millisecond { get; set; }

    public TimeSpan Second { get; set; }

    public TimeSpan Minute { get; set; }

    public TimeSpan Hour { get; set; }

    public TimeSpan? NullableWithoutValue { get; set; }

    public TimeSpan? NullableWithValue { get; set; }

    public TimeSpan? IsoSecond { get; set; }

    public TimeSpan? IsoMinute { get; set; }

    public TimeSpan? IsoHour { get; set; }

    public TimeSpan? IsoDay { get; set; }

    public TimeSpan? IsoMonth { get; set; }

    public TimeSpan? IsoYear { get; set; }
}

public class JsonEnumsTestStructure {
    public Disposition Upper { get; set; }

    public Disposition Lower { get; set; }

    public Disposition CamelCased { get; set; }

    public Disposition Underscores { get; set; }

    public Disposition LowerUnderscores { get; set; }

    public Disposition Dashes { get; set; }

    public Disposition LowerDashes { get; set; }

    public Disposition Integer { get; set; }
}

public class DecimalNumber {
    public decimal Value { get; set; }
}

public class Note {
    public const string TITLE   = "What a note.";
    public const string MESSAGE = "Content";

    [SerializeAs(Attribute = true), DeserializeAs(Attribute = true)]
    public int Id { get; set; }

    [SerializeAs(Content = true), DeserializeAs(Content = true)]
    public string Message { get; set; }

    public string Title { get; set; }
}

public class WrongNote {
    [SerializeAs(Content = true), DeserializeAs(Content = true)]
    public int Id { get; set; }

    [SerializeAs(Content = true), DeserializeAs(Content = true)]
    public string Text { get; set; }
}

public class DateTimeResponse {
    public DateTime CreatedOn { get; set; }
}