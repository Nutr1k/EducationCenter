using EducationCenter.Models;
using EducationCenterApi.Data.Abstract;
using EducationCenterApi.Enums;

namespace EducationCenterApi.Data.Implementation
{
	public class InMemoryLeadRepository : ILeadRepository
	{
		private readonly List<Lead> _leads = new();
		private readonly object _lock = new();
		private int _nextId = 1;

		public Task SaveAsync(Lead lead, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			lock (_lock)
			{
				var savedLead = new Lead
				{
					Id = _nextId++,
					Name = lead.Name,
					Phone = lead.Phone,
					Email = lead.Email,
					ClientType = lead.ClientType,
					Status = LeadStatus.Pending,
					CreatedAt = DateTime.UtcNow
				};

				_leads.Add(savedLead);
			}

			return Task.CompletedTask;
		}

		public Task<IReadOnlyCollection<Lead>> GetPendingAsync(CancellationToken cancellationToken)
		{

			cancellationToken.ThrowIfCancellationRequested();

			lock (_lock)
			{
				var pendingLeads = _leads
					.Where(lead => lead.Status == LeadStatus.Pending)
					.Select(lead => new Lead
					{
						Id = lead.Id,
						Name = lead.Name,
						Phone = lead.Phone,
						Email = lead.Email,
						ClientType = lead.ClientType,
						Status = lead.Status,
						CreatedAt = lead.CreatedAt
					}).ToList();

				return Task.FromResult<IReadOnlyCollection<Lead>>(pendingLeads);
			}
		}

		public Task MarkProcessedAsync(int id, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			lock (_lock)
			{
				var lead = _leads.FirstOrDefault(x => x.Id == id);

				if (lead != null)
				{
					lead.Status = LeadStatus.Processed;
				}
			}


			return Task.CompletedTask;
		}
	}
}
