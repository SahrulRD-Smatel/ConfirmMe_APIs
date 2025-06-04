using ConfirmMe.Data;
using ConfirmMe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static async Task Seed(AppDbContext context, IServiceProvider serviceProvider)
    {
        // Migrate database
        context.Database.Migrate();

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Seed Roles (harus sebelum users!)
        string[] roleNames = { "Staff", "Manager", "HRD", "Direktur" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Seed Positions
        if (!context.Positions.Any())
        {
            context.Positions.AddRange(
                new Position { Title = "Staff", ApprovalLevel = 1 },
                new Position { Title = "Manager", ApprovalLevel = 2 },
                new Position { Title = "HRD", ApprovalLevel = 3 },
                new Position { Title = "Direktur", ApprovalLevel = 4 }
            );
            await context.SaveChangesAsync();
        }

        // 3. Seed Approval Types
        if (!context.ApprovalTypes.Any())
        {
            context.ApprovalTypes.AddRange(
                new ApprovalType { Name = "Approval Only", Description = "Persetujuan tanpa tindak lanjut PO/Invoice" },
                new ApprovalType { Name = "Approval to PO", Description = "Persetujuan hingga PO" },
                new ApprovalType { Name = "Approval to Invoice", Description = "Persetujuan hingga Invoice" },
                new ApprovalType { Name = "Approval Bulk", Description = "Persetujuan massal" }
            );
            await context.SaveChangesAsync();
        }

        // 4. Seed Users
        if (!context.Users.Any())
        {
            await CreateUserAsync(userManager, context, "Arni Susanti Ndruru", "arni@infodata.co.id", "Password123@", "Staff");
            await CreateUserAsync(userManager, context, "Marwan Kosasih", "marwan@infodata.co.id", "Password123@", "HRD");
            await CreateUserAsync(userManager, context, "Sahrul Ramadhani", "sahrul@infodata.co.id", "Password123@", "Manager");
            await CreateUserAsync(userManager, context, "Sophia Arisanty", "sophia@infodata.co.id", "Password123@", "Direktur");
        }
    }

    private static async Task CreateUserAsync(
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    string fullName,
    string email,
    string password,
    string roleName)
    {
        var position = await context.Positions.FirstOrDefaultAsync(p => p.Title == roleName);

        if (position == null)
        {
            Console.WriteLine($"⚠️ WARNING: Position '{roleName}' not found. Skipping user creation for '{email}'.");
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            Console.WriteLine($"ℹ️ INFO: User with email '{email}' already exists. Skipping creation.");
            return;
        }

        var user = new ApplicationUser
        {
            FullName = fullName,
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            PositionId = position.Id
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            Console.WriteLine($"❌ ERROR: Failed to create user '{email}'. Errors: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            return;
        }

        var roleAssignResult = await userManager.AddToRoleAsync(user, roleName);
        if (!roleAssignResult.Succeeded)
        {
            Console.WriteLine($"❌ ERROR: Failed to assign role '{roleName}' to user '{email}'. Errors: {string.Join(", ", roleAssignResult.Errors.Select(e => e.Description))}");
        }
    }

}
