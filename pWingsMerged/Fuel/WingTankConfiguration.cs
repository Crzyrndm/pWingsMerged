using System.Collections.Generic;

namespace ProceduralWings.Fuel
{
    public class WingTankConfiguration : IConfigNode
    {
        public string ConfigurationName;
        public Dictionary<string, WingTankResource> resources = new Dictionary<string, WingTankResource>();

        public WingTankConfiguration(ConfigNode node)
        {
            Load(node);
        }

        public void Load(ConfigNode node)
        {
            float sum = 0;

            ConfigurationName = node.GetValue("tankLoadoutName");
            ConfigNode[] nodes = node.GetNodes("Resource");
            for (int i = 0; i < nodes.Length; ++i)
            {
                WingTankResource res = new WingTankResource(nodes[i]);
                resources.Add(res.resource.name, res);
                sum += res.ratio;
            }
            foreach (KeyValuePair<string, WingTankResource> kvp in resources)
            {
                kvp.Value.fraction = kvp.Value.ratio / sum;
            }
        }

        public void Save(ConfigNode node)
        {
            ConfigNode newNode = new ConfigNode("FuelSet");
            foreach (KeyValuePair<string, WingTankResource> kvp in resources)
            {
                kvp.Value.Save(newNode);
            }
            node.AddNode(newNode);
        }
    }
}