using CamelDotNet.Models.Base;
using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class Client : BaseModel, IEditable<Client>
    {
        public void Edit(Client model)
        {
            Name = model.Name;
        }
    }
}