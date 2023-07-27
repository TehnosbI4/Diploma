namespace MovementMonitoring.Utilities
{
    public static class TablesUpdateList
    {
        public static Dictionary<string, bool> Tables { get; } = new();


        public static void SetTableUpdateRequest(string table)
        {
            if (Tables.ContainsKey(table))
            {
                Tables[table] = true;
            }
            else
            {
                Tables.Add(table, true);
            }
        }
    }
}
