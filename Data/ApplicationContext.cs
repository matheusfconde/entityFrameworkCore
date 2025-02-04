﻿using CursoEFCore.Data.Configurations;
using CursoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CursoEFCore.Data
{
    public class ApplicationContext : DbContext
    {
        private static readonly ILoggerFactory _logger = LoggerFactory.Create(p => p.AddConsole()); // Log na aplicação.

        public DbSet<Pedido> Pedidos { get; set; } // Utilizar DBSet ou reescrever modelBuilder.
        public DbSet<Produto> Produtos { get; set; } // Utilizar DBSet ou reescrever modelBuilder.
        public DbSet<Cliente> Clientes { get; set; } // Utilizar DBSet ou reescrever modelBuilder.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(_logger).
                EnableSensitiveDataLogging().
                UseSqlServer("Server = 127.0.0.1, 1433; Database = CursoEFCore; User Id = SA; Password = R00t1234!;",
                p => p.EnableRetryOnFailure(
                    maxRetryCount: 2, 
                    maxRetryDelay: TimeSpan.FromSeconds(5), 
                    errorNumbersToAdd: null
                )
                .MigrationsHistoryTable("curso_ef_core")); // por padrão tenta seis vezes
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Maneira de configurar quais entidades a serem criadas uma por uma.
            //modelBuilder.ApplyConfiguration(new ClienteConfiguration());
            //modelBuilder.ApplyConfiguration(new PedidoConfiguration());
            //modelBuilder.ApplyConfiguration(new PedidoItemConfiguration());
            //modelBuilder.ApplyConfiguration(new ProdutoConfiguration());

            //Maneira de configurar todas as entidades de um assembly, a serem criadas.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationContext).Assembly);
            MapearPropriedadesEsquecidas(modelBuilder);

            //Maneira de configurar as entidades a serem criadas sobscrevendo o OnModelCreating.
            //modelBuilder.Entity<Cliente>(p =>
            //{
            //    p.ToTable("Clientes");
            //    p.HasKey(p => p.Id);
            //    p.Property(p => p.Nome).HasColumnType("VARCHAR(80)").IsRequired();
            //    p.Property(p => p.Telefone).HasColumnType("CHAR(11)");
            //    p.Property(p => p.CEP).HasColumnType("CHAR(8)").IsRequired();
            //    p.Property(p => p.Estado).HasColumnType("CHAR(2)").IsRequired();
            //    p.Property(p => p.Cidade).HasMaxLength(60).IsRequired(); //NVARCHAR(60)

            //    p.HasIndex(i => i.Telefone).HasName("idx_cliente_telefone");                
            //});

            //modelBuilder.Entity<Produto>(p =>
            //{
            //    p.ToTable("Produtos");
            //    p.HasKey(p => p.Id);
            //    p.Property(p => p.CodigoBarras).HasColumnType("VARCHAR(14)").IsRequired();
            //    p.Property(p => p.Descricao).HasColumnType("VARCHAR(60)");
            //    p.Property(p => p.Valor).IsRequired();
            //    p.Property(p => p.TipoProduto).HasConversion<string>();
            //});

            //modelBuilder.Entity<Pedido>(p =>
            //{
            //    p.ToTable("Pedidos");
            //    p.HasKey(p => p.Id);
            //    p.Property(p => p.IniciadoEm).HasDefaultValue("GETDATE()").ValueGeneratedOnAdd();
            //    p.Property(p => p.Status).HasConversion<string>();
            //    p.Property(p => p.TipoFrete).HasConversion<int>();
            //    p.Property(p => p.Observacao).HasColumnType("VARCHAR(512");

            //    p.HasMany(p => p.Itens)
            //        .WithOne(p => p.Pedido)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});

            //modelBuilder.Entity<PedidoItem>(p =>
            //{
            //    p.ToTable("PedidosItens");
            //    p.HasKey(p => p.Id);
            //    p.Property(p => p.Quantidade).HasDefaultValue(1).IsRequired();
            //    p.Property(p => p.Valor).IsRequired();
            //    p.Property(p => p.Desconto).IsRequired();
            //});

        }

        private static void MapearPropriedadesEsquecidas(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entity.GetProperties().Where(p => p.ClrType == typeof(string)); //filtrando as propriedades do tipo string.

                foreach (var property in properties)
                {
                    if (string.IsNullOrEmpty(property.GetColumnType())
                            && !property.GetMaxLength().HasValue)
                            property.SetMaxLength(100);
                            property.SetColumnType("VARCHAR(100)");
                }
            }
        }
    }
}
