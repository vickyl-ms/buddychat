using System.Collections.Generic;
using System;

namespace BuddyChatCLI
{
    public class SignupsConfig
    {
        public struct ConfigEntry
        {
            public string fieldName;

            // zero-based index
            public int index;
        }

        public int emailIndex {get; set;}

        public int nameIndex {get; set;}

        public IEnumerable<ConfigEntry> dataEntries {get; set;}

        public void ValidateConfig()
        {
            if (emailIndex < 0)
            {
                throw new Exception("emailIndex must be > 0");
            }

            if (nameIndex < 0)
            {
                throw new Exception("nameIndes must be > 0");
            }

            List<int> usedIndex = new List<int> {emailIndex};

            if (usedIndex.Contains(nameIndex))
            {
                throw new Exception("nameIndex has same value as another index.");
            }

            foreach (ConfigEntry entry in dataEntries)
            {
                if (ContainsWhiteSpace(entry.fieldName))
                {
                    throw new Exception($"FieldName '{entry.fieldName}' contains spaces or new line chars.");
                }

                if (usedIndex.Contains(entry.index))
                {
                    throw new Exception($"FieldName '{entry.fieldName}' has index '{entry.index}' that has already been used.");
                }

                usedIndex.Add(entry.index);
            }
        }

        private bool ContainsWhiteSpace(string fieldName)
        {
            return fieldName.Contains(' ') || fieldName.Contains('\n');
        }
    }
}