namespace ConfigApi.Context.Repositories;

public interface ICompanyRepository
{
    Task<string?> GetAllowedDomainsAsync(int companyId);
}