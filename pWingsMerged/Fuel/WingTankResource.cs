namespace ProceduralWings.Fuel
{
    public class WingTankResource : IConfigNode
    {
        public PartResourceDefinition resource;
        public float ratio; // resource units per 1m^3 of wing
        public float fraction; // the fraction of the volume used by this resource in this set. Calculated

        public WingTankResource(ConfigNode node)
        {
            Load(node);
        }

        public void Load(ConfigNode node)
        {
            resource = PartResourceLibrary.Instance.resourceDefinitions[node.GetValue("name").GetHashCode()];
            float.TryParse(node.GetValue("ratio"), out ratio);
        }

        public void Save(ConfigNode node)
        {
            ConfigNode newNode = new ConfigNode("Resource");
            newNode.AddValue("name", resource.name);
            newNode.AddValue("ratio", ratio);
            node.AddNode(newNode);
        }
    }
}