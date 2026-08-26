USE CloneEbayDB;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @DemoUsers TABLE(id int);
    INSERT INTO @DemoUsers SELECT id FROM [User] WHERE email IN (
        N'admin.demo@gmail.com',N'buyer1.demo@gmail.com',N'buyer2.demo@gmail.com',N'seller1.demo@gmail.com',N'seller2.demo@gmail.com',N'pending.demo@gmail.com',N'banned.demo@gmail.com',
        N'admin.demo@ebay.local',N'buyer1.demo@ebay.local',N'buyer2.demo@ebay.local',N'seller1.demo@ebay.local',N'seller2.demo@ebay.local',N'pending.demo@ebay.local',N'banned.demo@ebay.local');
    DECLARE @DemoProducts TABLE(id int);
    INSERT INTO @DemoProducts SELECT id FROM Product WHERE title IN (N'Demo Wireless Headphones',N'Demo Mechanical Keyboard',N'Demo Ceramic Coffee Set',N'Demo Vintage Camera Auction');
    DECLARE @DemoOrders TABLE(id int);
    INSERT INTO @DemoOrders SELECT id FROM OrderTable WHERE buyerId IN (SELECT id FROM @DemoUsers);

    DELETE FROM AdminAuditLog WHERE adminUserId IN (SELECT id FROM @DemoUsers) OR resourceId IN (SELECT id FROM @DemoUsers) AND resourceType=N'User';
    UPDATE [User] SET moderatedBy=NULL WHERE moderatedBy IN (SELECT id FROM @DemoUsers);
    UPDATE Product SET moderatedBy=NULL WHERE moderatedBy IN (SELECT id FROM @DemoUsers);
    UPDATE Review SET moderatedBy=NULL WHERE moderatedBy IN (SELECT id FROM @DemoUsers);
    UPDATE Dispute SET assignedTo=NULL,resolvedBy=NULL WHERE assignedTo IN (SELECT id FROM @DemoUsers) OR resolvedBy IN (SELECT id FROM @DemoUsers);
    DELETE FROM Dispute WHERE description LIKE N'DEMO DISPUTE %';
    DELETE FROM ReturnRequest WHERE reason LIKE N'DEMO RETURN %';
    DELETE FROM Review WHERE comment LIKE N'DEMO REVIEW:%';
    DELETE FROM ShippingInfo WHERE orderId IN (SELECT id FROM @DemoOrders);
    DELETE FROM Payment WHERE orderId IN (SELECT id FROM @DemoOrders);
    DELETE FROM OrderItem WHERE orderId IN (SELECT id FROM @DemoOrders);
    DELETE FROM OrderTable WHERE id IN (SELECT id FROM @DemoOrders);
    DELETE FROM Feedback WHERE sellerId IN (SELECT id FROM @DemoUsers);
    DELETE FROM Inventory WHERE productId IN (SELECT id FROM @DemoProducts);
    DELETE FROM Bid WHERE productId IN (SELECT id FROM @DemoProducts);
    DELETE FROM Coupon WHERE productId IN (SELECT id FROM @DemoProducts);
    DELETE FROM Product WHERE id IN (SELECT id FROM @DemoProducts);
    DELETE FROM Store WHERE storeName IN (N'Demo Tech Corner',N'Demo Living Market');
    DELETE FROM Address WHERE userId IN (SELECT id FROM @DemoUsers);
    DELETE FROM [User] WHERE id IN (SELECT id FROM @DemoUsers);
    DELETE FROM Category WHERE name IN (N'Demo Electronics',N'Demo Home',N'Demo Collectibles') AND NOT EXISTS (SELECT 1 FROM Product WHERE Product.categoryId=Category.id);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
