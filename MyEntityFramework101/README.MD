# Entity Framework experiments 101

# Overview
to be done

# How to specify primary key and indexes?
```
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.entity.Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Domain.entity.Address>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            base.OnModelCreating(modelBuilder);
        }

```

# How to view SQLite data?
### SQLiteStudio
https://sqlitestudio.pl/
