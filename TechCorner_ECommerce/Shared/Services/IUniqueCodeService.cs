namespace TechCorner_ECommerce.Services {
    public interface IUniqueCodeService {
        string CreateParentProductPublicId();
        string CreateSkuCode();
        string CreateOrderCode();
    }
}
