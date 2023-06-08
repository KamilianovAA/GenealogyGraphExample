namespace GenealogyGraph
{
    public class Name
    {
        public string? FirstName { get; set; }
        public List<string>? SurnameList { get; set; }
        public string? LastName { get; set; }

        public Name(string? firstName, List<string>? surnameList = null, string? lastName = "")
        {
            FirstName = firstName;
            SurnameList = surnameList;
            LastName = lastName;
        }
    }
}
