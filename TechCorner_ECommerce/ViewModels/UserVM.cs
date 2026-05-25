namespace TechCorner_ECommerce.ViewModels {
    public class UserListVM {
        public string Id { get; set; }

        public int No { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Role { get; set; }
    }

    public class EditUserVM {
        public string Id { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }


        public string? ReceiverName { get; set; }

        public string? Phone { get; set; }

        public string? FullAddress { get; set; }

        public string? RoleName { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}