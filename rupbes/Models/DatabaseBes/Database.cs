using System.Data.Entity;
using System.Configuration;

namespace rupbes.Models.DatabaseBes
{   
    public partial class Database : DbContext
    {

        static ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["DatabaseBes"];
        static string connStr = settings.ConnectionString.Insert(settings.ConnectionString.Length - 1, ";password=01011967");
        public Database()
            : base(connStr)
        {
        }

        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeCategory> EmployeeCategories { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }        
        public virtual DbSet<Tenders> Tenders { get; set; }
        public virtual DbSet<Vacation> Vacations { get; set; }

        public virtual DbSet<CompanyReview> CompanyReviews { get; set; }    

        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Besi_price> Besi_prices { get; set; }
        public virtual DbSet<Besi_price_list> Besi_price_lists { get; set; }
        public virtual DbSet<Besi_type_product> Besi_type_products { get; set; }
        public virtual DbSet<Besi_unit> Besi_units { get; set; }        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Организация
            modelBuilder.Entity<Company>().ToTable("RupbesOrg");
            modelBuilder.Entity<Company>().Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<Company>().Property(p => p.Name).HasColumnName("name");
            modelBuilder.Entity<Company>().Property(p => p.Abbreviation).HasColumnName("abbreviation");
            modelBuilder.Entity<Company>().Property(p => p.Unn).HasColumnName("unn");
            modelBuilder.Entity<Company>().Property(p => p.Okpo).HasColumnName("okpo");
            //Связи организации
            modelBuilder.Entity<Company>()
                .HasMany(e => e.Departments)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanId);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.CompanyReviews)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.IdCompany);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Contacts)
                .WithMany(e => e.Companies)
                .Map(m => m.ToTable("_RupbesOrgContact").MapLeftKey("RupbesOrgId").MapRightKey("ContactId"));
            //Отдел
            modelBuilder.Entity<Department>().ToTable("Department");
            modelBuilder.Entity<Department>().Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<Department>().Property(p => p.CompanId).HasColumnName("RupbesOrgId");
            modelBuilder.Entity<Department>().Property(p => p.Name).HasColumnName("name");
            //Связи отдела
            modelBuilder.Entity<Department>()
                .HasMany(e => e.Employees)
                .WithRequired(e => e.Department)
                .HasForeignKey(e => e.DepartmentId);

            modelBuilder.Entity<Department>()
                .HasMany(e => e.Contacts)
                .WithMany(e => e.Departments)
                .Map(m => m.ToTable("_DepartmentContact").MapLeftKey("DepartmentId").MapRightKey("ContactId"));
            //Сотрудник
            modelBuilder.Entity<Employee>().ToTable("Employee");
            modelBuilder.Entity<Employee>().Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<Employee>().Property(p => p.PostId).HasColumnName("ProffesionId");
            modelBuilder.Entity<Employee>().Property(p => p.CategoryId).HasColumnName("CategoryId");
            modelBuilder.Entity<Employee>().Property(p => p.DepartmentId).HasColumnName("DepartmentId");                        
            //Связи сотрудника
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Contacts)
                .WithMany(e => e.Employees)
                .Map(m => m.ToTable("_EmployeeContact").MapLeftKey("EmployeeId").MapRightKey("ContactId"));
            modelBuilder.Entity<Employee>()
            .HasOptional(e => e.EmployeeCategory)
            .WithMany()
            .HasForeignKey(e => e.CategoryId);
            //Категория сотрудника
            modelBuilder.Entity<EmployeeCategory>().ToTable("Employee_Category");
            modelBuilder.Entity<EmployeeCategory>().Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<EmployeeCategory>().Property(p => p.name).HasColumnName("name");
            //Должность
            modelBuilder.Entity<Post>().ToTable("Proffesion");
            modelBuilder.Entity<Post>().Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<Post>().Property(p => p.Name).HasColumnName("name");
            //Связи должности
            modelBuilder.Entity<Post>()
                .HasMany(e => e.Employees)
                .WithRequired(e => e.Post)
                .HasForeignKey(e => e.PostId);

            //Контакт
            modelBuilder.Entity<Contact>().ToTable("Contact");
            modelBuilder.Entity<Contact>().Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<Contact>().Property(p => p.ContactsTypeId).HasColumnName("TypeContactId");
            modelBuilder.Entity<Contact>().Property(p => p.ContactName).HasColumnName("contact");
            //Типы контактов
            modelBuilder.Entity<ContactsType>().ToTable("ContactType");
            modelBuilder.Entity<ContactsType>().Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<ContactsType>().Property(p => p.Type).HasColumnName("contact_type");
            //Связи типа контактов
            modelBuilder.Entity<ContactsType>()
                .HasMany(e => e.Contacts)
                .WithRequired(e => e.ContactsType)
                .HasForeignKey(e => e.ContactsTypeId);

            //Тендеры
            modelBuilder.Entity<Tenders>().ToTable("IcetradePurchase");    

            //Отзыв об организации
            modelBuilder.Entity<CompanyReview>().ToTable("Review");
            modelBuilder.Entity<CompanyReview>().Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<CompanyReview>().Property(p => p.IdCompany).HasColumnName("RupbesOrgId");            
            modelBuilder.Entity<CompanyReview>().Property(p => p.DateReview).HasColumnName("date");
            modelBuilder.Entity<CompanyReview>().Property(p => p.Confirm).HasColumnName("confirm");
            modelBuilder.Entity<CompanyReview>().Property(p => p.ReviewText).HasColumnName("review_text");     

            //документы
            modelBuilder.Entity<Document>().ToTable("Document");
            modelBuilder.Entity<Document>().Property(o => o.Id).HasColumnName("Id");
            modelBuilder.Entity<Document>().Property(o => o.Link).HasColumnName("link");
            modelBuilder.Entity<Document>().Property(o => o.NumberDocument).HasColumnName("number_document");
            modelBuilder.Entity<Document>().Property(o => o.DateDocument).HasColumnName("Date");

            modelBuilder.Entity<Document>()
                .HasMany(e => e.Besi_Type_Products_Photo)
                .WithRequired(e => e.Photo)
                .HasForeignKey(e => e.IdPhoto);

            modelBuilder.Entity<Document>()
                .HasMany(e => e.Besi_Type_Products_Drawing)
                .WithRequired(e => e.Drawing)
                .HasForeignKey(e => e.IdDrawing);

            //отпуска
            modelBuilder.Entity<Vacation>().ToTable("Vacation");
            modelBuilder.Entity<Vacation>().Property(o => o.Id).HasColumnName("Id");
            modelBuilder.Entity<Vacation>().Property(o => o.date_begin).HasColumnName("date_begin");
            modelBuilder.Entity<Vacation>().Property(o => o.date_end).HasColumnName("date_end");
            modelBuilder.Entity<Vacation>().Property(o => o.EmployeeId).HasColumnName("EmployeeId");

            modelBuilder.Entity<Vacation>()
                .HasRequired(e => e.Employee)
                .WithMany(e => e.Vacations)
                .HasForeignKey(e => e.EmployeeId);

            //Продукты БЭСИ
            modelBuilder.Entity<Besi_unit>().ToTable("Besi_Unit");
            modelBuilder.Entity<Besi_unit>().Property(o => o.Id).HasColumnName("Id");
            modelBuilder.Entity<Besi_unit>().Property(o => o.Name).HasColumnName("name");

            modelBuilder.Entity<Besi_unit>()
                .HasMany(e => e.Besi_Price_Lists)
                .WithRequired(e => e.Besi_unit)
                .HasForeignKey(e => e.IdBesi_unit);

            modelBuilder.Entity<Besi_type_product>().ToTable("Besi_type_product");
            modelBuilder.Entity<Besi_type_product>().Property(o => o.Id).HasColumnName("Id");
            modelBuilder.Entity<Besi_type_product>().Property(o => o.Name).HasColumnName("name");
            modelBuilder.Entity<Besi_type_product>().Property(o => o.IdPhoto).HasColumnName("doc_photo");
            modelBuilder.Entity<Besi_type_product>().Property(o => o.IdDrawing).HasColumnName("doc_drawing");

            modelBuilder.Entity<Besi_type_product>()
                .HasMany(e => e.Besi_Price_Lists)
                .WithRequired(e => e.Besi_type_product)
                .HasForeignKey(e => e.IdBesi_type_product);

            modelBuilder.Entity<Besi_price>().ToTable("Besi_Price");
            modelBuilder.Entity<Besi_price>().Property(o => o.Id).HasColumnName("Id");
            modelBuilder.Entity<Besi_price>().Property(o => o.Price).HasColumnName("price");
            modelBuilder.Entity<Besi_price>().Property(o => o.Price_w_nds).HasColumnName("price_w_nds");
            modelBuilder.Entity<Besi_price>().Property(o => o.DatePrice).HasColumnName("Date");
            modelBuilder.Entity<Besi_price>().Property(o => o.IdBesi_price_list).HasColumnName("BesiPriceListId");

            modelBuilder.Entity<Besi_price_list>().ToTable("Besi_price_list ");
            modelBuilder.Entity<Besi_price_list>().Property(o => o.Id).HasColumnName("Id");
            modelBuilder.Entity<Besi_price_list>().Property(o => o.Number).HasColumnName("number");
            modelBuilder.Entity<Besi_price_list>().Property(o => o.Name).HasColumnName("name");
            modelBuilder.Entity<Besi_price_list>().Property(o => o.Volume).HasColumnName("volume");
            modelBuilder.Entity<Besi_price_list>().Property(o => o.Weight).HasColumnName("weight");
            modelBuilder.Entity<Besi_price_list>().Property(o => o.IdBesi_unit).HasColumnName("BesiUnitId");
            modelBuilder.Entity<Besi_price_list>().Property(o => o.IdBesi_type_product).HasColumnName("BesiTypeId");

            modelBuilder.Entity<Besi_price_list>()
                .HasMany(e => e.Besi_Prices)
                .WithRequired(e => e.Besi_price_list)
                .HasForeignKey(e => e.IdBesi_price_list);            
        }      

        public System.Data.Entity.DbSet<rupbes.Models.DatabaseBes.ContactsType> ContactsTypes { get; set; }
    }
}
