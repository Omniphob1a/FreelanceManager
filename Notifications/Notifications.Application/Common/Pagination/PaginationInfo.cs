public record PaginationInfo(
	int TotalItems,
	int ItemsPerPage,
	int ActualPage
)
{
	public PaginationInfo() : this(0, 10, 1) { }

	public int NormalizedItemsPerPage => ItemsPerPage switch
	{
		< 1 => 10,
		> 100 => 100,
		_ => ItemsPerPage
	};

	public int NormalizedPage => ActualPage < 1 ? 1 : ActualPage;

	public int TotalPages => (int)Math.Ceiling((double)(TotalItems < 0 ? 0 : TotalItems) / NormalizedItemsPerPage);
}