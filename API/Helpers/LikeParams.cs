namespace API.Helpers
{
    public class LikeParams : PaginationParams
    {
        public string predicate { get; set; }
        public int userId { get; set; }


    }
}