namespace TechCorner_ECommerce.ViewModels {
    public class RoleVM {
        public string Id { get; set; }

        public int No { get; set; }

        public string Name { get; set; }

        public int UserCount { get; set; }
    }

    public class RoleCreateVM {
        public string Name { get; set; }
    }

    public class RoleEditVM {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}