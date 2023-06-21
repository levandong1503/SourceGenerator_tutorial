using SourceGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySourceGenerator2
{
    [EntityToDto(typeof(BookEntity))]
    public partial class BookGenerateDto
    {
    }
}
