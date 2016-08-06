namespace ProceduralWings
{
    public class WingProperty :IConfigNode
    {
        public string name;
        public double value;
        public double defaultValue;
        public int decPlaces;
        public double min;
        public double max;

        public WingProperty(string Name, double DefaultV = 0, int DecPlaces = 2, double Min = 0, double Max = 0)
        {
            name = Name;
            value = defaultValue = DefaultV;
            decPlaces = DecPlaces;
            min = Min;
            max = Max;
        }

        public WingProperty(WingProperty prop)
        {
            Update(prop);
        }

        public void Update(WingProperty prop)
        {
            name = prop.name;
            value = prop.value;
            defaultValue = prop.defaultValue;
            decPlaces = prop.decPlaces;
            min = prop.min;
            max = prop.max;
        }

        public void Load(ConfigNode node)
        {
            node.TryGetValue("name", ref name);
            node.TryGetValue("value", ref value);
        }

        public void Save(ConfigNode node)
        {
            ConfigNode pNode = new ConfigNode("WING_PROPERTY");
            pNode.AddValue("name", name);
            pNode.AddValue("value", value);
            node.AddNode(pNode);
        }
    }
}
