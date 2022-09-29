namespace MHR_TU2_Fixer.MDF
{
    public class BooleanHolder
    {
        public string Name { get; set; }

        public bool Selected { get; set; }

        public BooleanHolder(string name, bool selected)
        {
            Name = name;
            Selected = selected;
        }
    }
}