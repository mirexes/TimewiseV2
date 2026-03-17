using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class CompanyChatConfiguration : IEntityTypeConfiguration<CompanyChat>
{
    public void Configure(EntityTypeBuilder<CompanyChat> builder)
    {
        builder.ToTable("CompanyChats");

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
    }
}
