using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.QFTemplates
{
    [Table("pets")]
    public class PetProfile
    {
        [SqlLikeTerm]
        public string? Name { get; set; }

        [SqlLikeTerm]
        public string? Species { get; set; }
    }
}
