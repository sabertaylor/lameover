using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lameover
{
    public static class Configuration
    {
        private const string configurationFile = @"configuration.xml";

        public static void Save(Diversions diversions)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Diversions));
            using (TextWriter writer = new StreamWriter(configurationFile))
            {
                serializer.Serialize(writer, diversions);
            }
        }

        public static Diversions Load()
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Diversions));

            Diversions justNames;
            using (TextReader reader = new StreamReader(configurationFile))
            {
                justNames = (Diversions)serializer.Deserialize(reader);
            }

            Diversions diversions = new Diversions();
            foreach (var process in justNames.Processes)
            {
                diversions.AddDiversion(process.Process, 0);
            }

            return diversions;
        }
    }
}
