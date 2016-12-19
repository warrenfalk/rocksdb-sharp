using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public class ColumnFamilies : IEnumerable<ColumnFamilies.Descriptor>
    {
        private List<Descriptor> descriptors { get; } = new List<Descriptor>();

        public static readonly string DefaultName = "default";

        public class Descriptor
        {
            public string Name { get; }
            public ColumnFamilyOptions Options { get; }

            public Descriptor(string name, ColumnFamilyOptions options)
            {
                this.Name = name;
                this.Options = options;
            }
        }

        public ColumnFamilies(ColumnFamilyOptions options = null)
        {
            descriptors.Add(new Descriptor(DefaultName, options ?? new ColumnFamilyOptions()));
        }

        public void Add(Descriptor descriptor)
        {
            if (descriptor.Name == DefaultName)
                descriptors[0] = descriptor;
            else
                descriptors.Add(descriptor);
        }

        public void Add(string name, ColumnFamilyOptions options)
        {
            Add(new Descriptor(name, options));
        }

        public IEnumerator<Descriptor> GetEnumerator()
        {
            return descriptors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return descriptors.GetEnumerator();
        }
    }
}
