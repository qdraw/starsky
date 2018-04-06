using System.Collections.Generic;
using starsky.Services;

namespace starsky.Models
{
    public class BasePathConfig
    {
        private List<string> _readonly;
        public List<string> Readonly
        {
            get => _readonly;
            set
            {
                _readonly = new List<string>();

                if (value == null) return;

                foreach (var item in value)
                {
                    _readonly.Add(ConfigRead.RemoveLatestBackslash(item));
                }
            }
        }
    }
}