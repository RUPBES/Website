namespace rupbes.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Configuration;
    using rupbes.Models.DatabaseBes;
    using rupbes.Models.Products;

    public partial class Database : DbContext
    {
        static ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["Database"];
        static string connStr = settings.ConnectionString.Insert(settings.ConnectionString.Length - 1, ";password=01011967");
        public Database()
            : base(connStr)
        {
        }

        public virtual DbSet<Bosses> Bosses { get; set; }
        public virtual DbSet<Certificates> Certificates { get; set; }
        public virtual DbSet<Contacts> Contacts { get; set; }
        public virtual DbSet<Dep_groups> Dep_groups { get; set; }
        public virtual DbSet<Dep_types> Dep_types { get; set; }
        public virtual DbSet<Departments> Departments { get; set; }
        public virtual DbSet<Deps_to_groups> Deps_to_groups { get; set; }
        public virtual DbSet<Img_types> Img_types { get; set; }
        public virtual DbSet<Imgs> Imgs { get; set; }
        public virtual DbSet<Mechanisms> Mechanisms { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<News_type> News_type { get; set; }
        public virtual DbSet<Objects> Objects { get; set; }
        public virtual DbSet<Realty> Realty { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Services> Services { get; set; }
        public virtual DbSet<Usage_report> Usage_report { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Vacancies> Vacancies { get; set; }
        public virtual DbSet<Sale> Sale { get; set; }

        public virtual DbSet<Component> Components { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Property> Properties { get; set; }
        public virtual DbSet<GroupProduct> GroupProducts { get; set; }
        public virtual DbSet<SubGroupProduct> SubGroupProducts { get; set; }
        public virtual DbSet<PropertyProduct> PropertyProducts { get; set; }
        public virtual DbSet<PropertyVersion> PropertyVersions { get; set; }
        public virtual DbSet<VersionProduct> VersionProducts { get; set; }
        public virtual DbSet<ComponentProduct> ComponentProducts { get; set; }
        public virtual DbSet<Imgs_to_product> ImgsProduct { get; set; }
        public virtual DbSet<Imgs_to_versionProduct> ImgsVersionProduct { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Certificates>()
                .HasMany(e => e.Imgs)
                .WithMany(e => e.Certificates)
                .Map(m => m.ToTable("Imgs_to_cerificate", "rupbesby_admin").MapLeftKey("id_certificate").MapRightKey("id_img"));

            modelBuilder.Entity<Dep_groups>()
                .HasMany(e => e.Deps_to_groups)
                .WithRequired(e => e.Dep_groups)
                .HasForeignKey(e => e.id_group);

            modelBuilder.Entity<Dep_types>()
                .HasMany(e => e.Departments)
                .WithRequired(e => e.Dep_types)
                .HasForeignKey(e => e.type_id);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Bosses)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Certificates)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Deps_to_groups)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Mechanisms)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.News)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Realty)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep);
            
            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Sale)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Services)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Users)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Vacancies)
                .WithRequired(e => e.Departments)
                .HasForeignKey(e => e.id_dep);

            modelBuilder.Entity<Departments>()
                .HasMany(e => e.Objects)
                .WithMany(e => e.Departments)
                .Map(m => m.ToTable("Deps_to_objects", "rupbesby_admin").MapLeftKey("id_dep").MapRightKey("id_obj"));

            modelBuilder.Entity<Deps_to_groups>()
                .HasOptional(e => e.Contacts)
                .WithRequired(e => e.Deps_to_groups)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Img_types>()
                .HasMany(e => e.Imgs)
                .WithOptional(e => e.Img_types)
                .HasForeignKey(e => e.type_id)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Imgs>()
                .HasMany(e => e.Bosses)
                .WithRequired(e => e.Imgs)
                .HasForeignKey(e => e.id_img)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Imgs>()
                .HasMany(e => e.Departments)
                .WithOptional(e => e.Imgs)
                .HasForeignKey(e => e.id_img)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Imgs>()
                .HasMany(e => e.Mechanisms)
                .WithMany(e => e.Imgs)
                .Map(m => m.ToTable("Imgs_to_mechanisms", "rupbesby_admin").MapLeftKey("id_img").MapRightKey("id_mech"));

            modelBuilder.Entity<Imgs>()
                .HasMany(e => e.News)
                .WithMany(e => e.Imgs)
                .Map(m => m.ToTable("Imgs_to_news", "rupbesby_admin").MapLeftKey("id_img").MapRightKey("id_news"));

            modelBuilder.Entity<Imgs>()
                .HasMany(e => e.Objects)
                .WithMany(e => e.Imgs)
                .Map(m => m.ToTable("Imgs_to_objects", "rupbesby_admin").MapLeftKey("id_img").MapRightKey("id_object"));

            modelBuilder.Entity<Imgs>()
                .HasMany(e => e.Realty)
                .WithMany(e => e.Imgs)
                .Map(m => m.ToTable("Imgs_to_realty", "rupbesby_admin").MapLeftKey("id_img").MapRightKey("id_realty"));
            
            modelBuilder.Entity<Imgs>()
                .HasMany(e => e.Sale)
                .WithMany(e => e.Imgs)
                .Map(m => m.ToTable("Imgs_to_sale", "rupbesby_admin").MapLeftKey("id_img").MapRightKey("id_sale"));

            modelBuilder.Entity<Imgs>()
                .HasMany(e => e.Services)
                .WithMany(e => e.Imgs)
                .Map(m => m.ToTable("Imgs_to_services", "rupbesby_admin").MapLeftKey("id_img").MapRightKey("id_service"));

            modelBuilder.Entity<News>()
                .Property(e => e.body_ru)
                .IsUnicode(false);

            modelBuilder.Entity<News>()
                .Property(e => e.body_bel)
                .IsUnicode(false);

            modelBuilder.Entity<News_type>()
                .HasMany(e => e.News)
                .WithRequired(e => e.News_type)
                .HasForeignKey(e => e.type_id);

            modelBuilder.Entity<Objects>()
                .Property(e => e.desc_ru)
                .IsUnicode(false);

            modelBuilder.Entity<Objects>()
                .Property(e => e.desc_bel)
                .IsUnicode(false);

            modelBuilder.Entity<Roles>()
                .HasMany(e => e.Users)
                .WithRequired(e => e.Roles)
                .HasForeignKey(e => e.id_role);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.Usage_report)
                .WithRequired(e => e.Users)
                .HasForeignKey(e => e.id_user);

            modelBuilder.Entity<PropertyVersion>()
                .HasKey(e => new { e.PropertyId, e.VersionId });
            modelBuilder.Entity<PropertyProduct>()
                .HasKey(e => new { e.PropertyId, e.ProductId });
            modelBuilder.Entity<ComponentProduct>()
                .HasKey(e => new { e.ComponentId, e.ProductId });
            modelBuilder.Entity<Imgs_to_product>()
                .HasKey(e => new { e.ImgsId, e.ProductId });
            modelBuilder.Entity<Imgs_to_versionProduct>()
                .HasKey(e => new { e.ImgsId, e.VersionProductId });

            modelBuilder.Entity<GroupProduct>().ToTable("GroupProduct", "rupbesby_admin");
            modelBuilder.Entity<SubGroupProduct>().ToTable("SubGroupProduct", "rupbesby_admin");
            modelBuilder.Entity<Product>().ToTable("Product", "rupbesby_admin");
            modelBuilder.Entity<VersionProduct>().ToTable("VersionProduct", "rupbesby_admin");
            modelBuilder.Entity<PropertyProduct>().ToTable("PropertyProduct", "rupbesby_admin");
            modelBuilder.Entity<PropertyVersion>().ToTable("PropertyVersion", "rupbesby_admin");
            modelBuilder.Entity<ComponentProduct>().ToTable("ComponentProduct", "rupbesby_admin");
            modelBuilder.Entity<Component>().ToTable("Component", "rupbesby_admin");
            modelBuilder.Entity<Property>().ToTable("Property", "rupbesby_admin");
            modelBuilder.Entity<Unit>().ToTable("Unit", "rupbesby_admin");

            modelBuilder.Entity<Imgs_to_product>().ToTable("Imgs_to_product", "rupbesby_admin");
            modelBuilder.Entity<Imgs_to_product>().Property(p => p.ProductId).HasColumnName("id_product");
            modelBuilder.Entity<Imgs_to_product>().Property(p => p.ImgsId).HasColumnName("id_img");

            modelBuilder.Entity<Imgs_to_versionProduct>().ToTable("Imgs_to_versionProduct", "rupbesby_admin");
            modelBuilder.Entity<Imgs_to_versionProduct>().Property(p => p.VersionProductId).HasColumnName("id_versionProduct");
            modelBuilder.Entity<Imgs_to_versionProduct>().Property(p => p.ImgsId).HasColumnName("id_img");
        }
    }
}
