namespace WinFormsApp1
{
    public class EncodingItem
    {
        public int CodePage { get; set; }
        public string Name { get; set; }

        public EncodingItem() { }

        public EncodingItem(int codePage, string name)
        {
            CodePage = codePage;
            Name = name;
        }

        public override string ToString()
        {
            return $"{CodePage}-{Name}";
        }
    }
}
