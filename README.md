# FlexRepo

FlexRepo é uma biblioteca de repositório genérico para .NET, projetada para fornecer uma interface consistente e reutilizável para acesso a dados, facilitando operações CRUD (Criar, Ler, Atualizar, Deletar) e paginação em bancos de dados. A biblioteca é compatível com `net8.0`, permitindo que seja utilizada em uma ampla gama de projetos .NET.

## Recursos

- Repositório genérico para operações CRUD.
- Suporte a paginação.
- Consultas personalizáveis com expressões lambda.
- Suporte para incluir propriedades relacionadas.
- Implementação assíncrona.

## Instalação

Para instalar a FlexRepo, você pode usar o NuGet Package Manager:

```bash
dotnet add package FlexRepo
```

## Como Usar
### 1. Crie seu DbContext

Defina seu contexto de banco de dados que herda de DbContext.

```csharp

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().HasKey(u => u.Id);
    }
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}
```

### 2. Configure o Repositório Específico

Implemente o repositório genérico com o seu DbContext.

```csharp
using FlexRepo.Repositories;

public class UserRepository : Repository<User, Guid, AppDbContext>
{
    public UserRepository(AppDbContext context) : base(context) { }
}
```

### 3. Configure a Injeção de Dependência
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

    services.AddScoped<IRepository<User, Guid>, UserRepository>();
}
```

### 4. Use o Repositório em seu Serviço

Use o repositório genérico em seus serviços para realizar operações CRUD.

```csharp
public class UserService
{
    private readonly IRepository<User, Guid> _userRepository;

    public UserService(IRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetSingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task<PaginatedList<User>> GetUsersAsync(int pageIndex, int pageSize)
    {
        return await _userRepository.GetPaginatedAsync(pageIndex, pageSize);
    }

    public async Task AddUserAsync(User user)
    {
        await _userRepository.AddAsync(user, saveChanges: true);
    }

    public async Task UpdateUserAsync(User user)
    {
        await _userRepository.UpdateAsync(user, saveChanges: true);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        await _userRepository.DeleteAsync(userId, saveChanges: true);
    }
}
```

### 5. Configuração do TestDbContext

Defina um contexto de banco de dados para testes usando o provedor de banco de dados em memória.

```csharp
using Microsoft.EntityFrameworkCore;

namespace FlexRepo.Tests
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
```

### 6. Configuração de Testes

Adicione testes unitários para garantir que seu repositório funcione corretamente.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlexRepo.Tests
{
    public class RepositoryTests
    {
        private readonly TestDbContext _context;
        private readonly UserRepository _repository;

        public RepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TestDbContext(options);
            _repository = new UserRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddUser()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Name = "Test User" };

            await _repository.AddAsync(user, saveChanges: true);
            var retrievedUser = await _repository.GetByIdAsync(user.Id);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Email, retrievedUser?.Email);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Name = "Test User" };
            await _repository.AddAsync(user, saveChanges: true);

            var retrievedUser = await _repository.GetByIdAsync(user.Id);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Email, retrievedUser?.Email);
        }

        [Fact]
        public async Task GetPaginatedAsync_ShouldReturnPaginatedUsers()
        {
            for (int i = 0; i < 10; i++)
            {
                await _repository.AddAsync(new User { Id = Guid.NewGuid(), Email = $"test{i}@example.com", Name = $"Test User {i}" }, saveChanges: true);
            }

            var paginatedUsers = await _repository.GetPaginatedAsync(1, 5);

            Assert.Equal(5, paginatedUsers.Items.Count);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveUser()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Name = "Test User" };
            await _repository.AddAsync(user, saveChanges: true);

            await _repository.DeleteAsync(user.Id, saveChanges: true);
            var retrievedUser = await _repository.GetByIdAsync(user.Id);

            Assert.Null(retrievedUser);
        }
    }
}
```

### 7. Extensões para IQueryable

Utilize a classe de extensão para facilitar a inclusão de propriedades relacionadas.

```csharp

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace FlexRepo.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> query, string includeProperties) where T : class
        {
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            return query;
        }
    }
}
```

### Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir um problema ou enviar uma solicitação pull.

### Licença

Este projeto está licenciado sob a MIT License.