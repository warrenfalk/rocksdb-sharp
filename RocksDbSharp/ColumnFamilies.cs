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
        private List<Descriptor> Descriptors { get; } = new List<Descriptor>();

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
            Descriptors.Add(new Descriptor(DefaultName, options ?? new ColumnFamilyOptions()));
        }

        public IEnumerable<string> Names => this.Select(cfd => cfd.Name);

        public IEnumerable<IntPtr> OptionHandles => this.Select(cfd => cfd.Options.Handle);

        public void Add(Descriptor descriptor)
        {
            if (descriptor.Name == DefaultName)
                Descriptors[0] = descriptor;
            else
                Descriptors.Add(descriptor);
        }

        public void Add(string name, ColumnFamilyOptions options)
        {
            Add(new Descriptor(name, options));
        }

        public IEnumerator<Descriptor> GetEnumerator()
        {
            return Descriptors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Descriptors.GetEnumerator();
        }
    }
}
