namespace GenealogyGraph
{
    public class Family
    {
        public int Id { get; set; }
        public Person? Father { get; set; }
        public Person? Mother { get; set; }
        public List<Person>? Children { get; set; }
        public FamilyRelationType Type { get; set; }

        public Person Person
        {
            get => default;
            set
            {
            }
        }

        public FamilyRelationType FamilyRelationType
        {
            get => default;
            set
            {
            }
        }

        public Family() 
        {
            
        }
    }
}