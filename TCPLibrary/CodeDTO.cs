
namespace TCPLibrary
{
    public record CodeDTO
    {
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public CodeDTO (string code, bool _isActive)
        {
            Code = code;
            IsActive = _isActive;
        }
    }
}
