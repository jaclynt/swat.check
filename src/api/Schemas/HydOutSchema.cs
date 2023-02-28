namespace SWAT.Check.Schemas;

public static class HydOutSchema
{
    public static SchemaLine ICode = new() { StartIndex = 0, Length = 8 };
    public static SchemaLine Flow = new() { StartIndex = 76, Length = 12 };
    public static SchemaLine Sed = new() { StartIndex = 88, Length = 12 };
    public static SchemaLine OrgN = new() { StartIndex = 100, Length = 12 };
    public static SchemaLine OrgP = new() { StartIndex = 112, Length = 12 };
    public static SchemaLine Nitrate = new() { StartIndex = 124, Length = 12 };
    public static SchemaLine SolP = new() { StartIndex = 136, Length = 12 };
    public static SchemaLine SolPst = new() { StartIndex = 148, Length = 12 };
    public static SchemaLine SorPst = new() { StartIndex = 160, Length = 12 };
}
