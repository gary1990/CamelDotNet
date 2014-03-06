using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelDotNet.Models.Interfaces
{
    public interface IEditable<Model>
    {
        void Edit(Model model);
    }
}
