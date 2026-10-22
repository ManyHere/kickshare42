namespace _42v2
{
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static string Name { get; set; }
        public static string Role { get; set; }
        public static string Phone { get; set; }

        public static void Reset()
        {
            Id = 0;
            Name = "";
            Role = "";
            Phone = "";
        }
    }
}
