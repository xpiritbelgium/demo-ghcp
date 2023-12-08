using CleanArchitecture.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Persistence.Documents
{
    public class PublishedDocumentEntityConfiguration : IEntityTypeConfiguration<PublishedDocument>
    {

        public void Configure(EntityTypeBuilder<PublishedDocument> builder)
        {
            builder.ToTable("PublishedDocuments", "pub");

            builder.HasKey(x => x.Id);
        }
    }
}
