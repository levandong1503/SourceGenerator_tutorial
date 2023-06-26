using Nws.AbpSourceGenerator;

namespace MySourceGenerator2;


[PropertiesFrom<BookEntity>(ignores:  new string[] { nameof(BookEntity.PublishDate) })]
public partial class BookGenerateDto
{
}

