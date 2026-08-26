USE CloneEbayDB;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @PasswordHash nvarchar(255) = N'$2a$11$PyTK/zDoZCSYTOWfzejf1.kaAxdNufBCHLWoD7TaTre1Q90bDRM/y';

    IF NOT EXISTS (SELECT 1 FROM [User] WHERE email = N'admin.demo@gmail.com') INSERT INTO [User] (username,email,password,role) VALUES (N'Demo Admin',N'admin.demo@gmail.com',@PasswordHash,N'Admin');
    IF NOT EXISTS (SELECT 1 FROM [User] WHERE email = N'buyer1.demo@gmail.com') INSERT INTO [User] (username,email,password,role) VALUES (N'Demo Buyer A',N'buyer1.demo@gmail.com',@PasswordHash,N'User');
    IF NOT EXISTS (SELECT 1 FROM [User] WHERE email = N'buyer2.demo@gmail.com') INSERT INTO [User] (username,email,password,role) VALUES (N'Demo Buyer B',N'buyer2.demo@gmail.com',@PasswordHash,N'User');
    IF NOT EXISTS (SELECT 1 FROM [User] WHERE email = N'seller1.demo@gmail.com') INSERT INTO [User] (username,email,password,role) VALUES (N'Demo Seller A',N'seller1.demo@gmail.com',@PasswordHash,N'Seller');
    IF NOT EXISTS (SELECT 1 FROM [User] WHERE email = N'seller2.demo@gmail.com') INSERT INTO [User] (username,email,password,role) VALUES (N'Demo Seller B',N'seller2.demo@gmail.com',@PasswordHash,N'Seller');
    IF NOT EXISTS (SELECT 1 FROM [User] WHERE email = N'pending.demo@gmail.com') INSERT INTO [User] (username,email,password,role,moderationStatus) VALUES (N'Demo Pending User',N'pending.demo@gmail.com',@PasswordHash,N'User',N'Pending');
    IF NOT EXISTS (SELECT 1 FROM [User] WHERE email = N'banned.demo@gmail.com') INSERT INTO [User] (username,email,password,role,moderationStatus,moderationReason,moderatedAtUtc) VALUES (N'Demo Banned User',N'banned.demo@gmail.com',@PasswordHash,N'User',N'Banned',N'DEMO BAN: repeated policy violations.',SYSUTCDATETIME());

    DECLARE @BuyerA int = (SELECT id FROM [User] WHERE email=N'buyer1.demo@gmail.com');
    DECLARE @BuyerB int = (SELECT id FROM [User] WHERE email=N'buyer2.demo@gmail.com');
    DECLARE @SellerA int = (SELECT id FROM [User] WHERE email=N'seller1.demo@gmail.com');
    DECLARE @SellerB int = (SELECT id FROM [User] WHERE email=N'seller2.demo@gmail.com');
    UPDATE [User] SET moderationStatus=N'Active',moderationReason=NULL,moderatedBy=NULL,moderatedAtUtc=NULL WHERE email IN (N'admin.demo@gmail.com',N'buyer1.demo@gmail.com',N'buyer2.demo@gmail.com',N'seller1.demo@gmail.com',N'seller2.demo@gmail.com');
    UPDATE [User] SET moderationStatus=N'Pending',moderationReason=NULL,moderatedBy=NULL,moderatedAtUtc=NULL WHERE email=N'pending.demo@gmail.com';
    UPDATE [User] SET moderationStatus=N'Banned',moderationReason=N'DEMO BAN: repeated policy violations.',moderatedBy=NULL,moderatedAtUtc=SYSUTCDATETIME() WHERE email=N'banned.demo@gmail.com';

    IF NOT EXISTS (SELECT 1 FROM Category WHERE name=N'Demo Electronics') INSERT INTO Category(name) VALUES(N'Demo Electronics');
    IF NOT EXISTS (SELECT 1 FROM Category WHERE name=N'Demo Home') INSERT INTO Category(name) VALUES(N'Demo Home');
    IF NOT EXISTS (SELECT 1 FROM Category WHERE name=N'Demo Collectibles') INSERT INTO Category(name) VALUES(N'Demo Collectibles');
    DECLARE @Electronics int=(SELECT id FROM Category WHERE name=N'Demo Electronics');
    DECLARE @Home int=(SELECT id FROM Category WHERE name=N'Demo Home');
    DECLARE @Collectibles int=(SELECT id FROM Category WHERE name=N'Demo Collectibles');

    IF NOT EXISTS (SELECT 1 FROM Store WHERE storeName=N'Demo Tech Corner') INSERT INTO Store(sellerId,storeName,description,bannerImageURL) VALUES(@SellerA,N'Demo Tech Corner',N'Reliable electronics from Seller A.',N'/images/demo-tech.jpg');
    IF NOT EXISTS (SELECT 1 FROM Store WHERE storeName=N'Demo Living Market') INSERT INTO Store(sellerId,storeName,description,bannerImageURL) VALUES(@SellerB,N'Demo Living Market',N'Home goods and collectibles from Seller B.',N'/images/demo-living.jpg');

    IF NOT EXISTS (SELECT 1 FROM Address WHERE userId=@BuyerA AND street=N'101 Demo Street') INSERT INTO Address(userId,fullName,phone,street,city,state,country,isDefault) VALUES(@BuyerA,N'Demo Buyer A',N'0900000001',N'101 Demo Street',N'Ho Chi Minh City',N'HCMC',N'Vietnam',1);
    IF NOT EXISTS (SELECT 1 FROM Address WHERE userId=@BuyerB AND street=N'202 Demo Avenue') INSERT INTO Address(userId,fullName,phone,street,city,state,country,isDefault) VALUES(@BuyerB,N'Demo Buyer B',N'0900000002',N'202 Demo Avenue',N'Ha Noi',N'Ha Noi',N'Vietnam',1);
    DECLARE @AddressA int=(SELECT id FROM Address WHERE userId=@BuyerA AND street=N'101 Demo Street');
    DECLARE @AddressB int=(SELECT id FROM Address WHERE userId=@BuyerB AND street=N'202 Demo Avenue');

    IF NOT EXISTS (SELECT 1 FROM Product WHERE title=N'Demo Wireless Headphones') INSERT INTO Product(title,description,price,images,categoryId,sellerId,isAuction,auctionEndTime) VALUES(N'Demo Wireless Headphones',N'Noise cancelling headphones.',129.99,N'["/images/headphones.jpg"]',@Electronics,@SellerA,0,NULL);
    IF NOT EXISTS (SELECT 1 FROM Product WHERE title=N'Demo Mechanical Keyboard') INSERT INTO Product(title,description,price,images,categoryId,sellerId,isAuction,auctionEndTime) VALUES(N'Demo Mechanical Keyboard',N'Hot-swappable keyboard.',89.50,N'["/images/keyboard.jpg"]',@Electronics,@SellerA,0,NULL);
    IF NOT EXISTS (SELECT 1 FROM Product WHERE title=N'Demo Ceramic Coffee Set') INSERT INTO Product(title,description,price,images,categoryId,sellerId,isAuction,auctionEndTime) VALUES(N'Demo Ceramic Coffee Set',N'Six-piece ceramic set.',59.00,N'["/images/coffee-set.jpg"]',@Home,@SellerB,0,NULL);
    IF NOT EXISTS (SELECT 1 FROM Product WHERE title=N'Demo Vintage Camera Auction') INSERT INTO Product(title,description,price,images,categoryId,sellerId,isAuction,auctionEndTime) VALUES(N'Demo Vintage Camera Auction',N'Working vintage film camera.',220.00,N'["/images/vintage-camera.jpg"]',@Collectibles,@SellerB,1,DATEADD(day,7,GETDATE()));
    DECLARE @Headphones int=(SELECT id FROM Product WHERE title=N'Demo Wireless Headphones');
    DECLARE @Keyboard int=(SELECT id FROM Product WHERE title=N'Demo Mechanical Keyboard');
    DECLARE @Coffee int=(SELECT id FROM Product WHERE title=N'Demo Ceramic Coffee Set');
    DECLARE @Camera int=(SELECT id FROM Product WHERE title=N'Demo Vintage Camera Auction');
    UPDATE Product SET moderationStatus=N'Active', moderationReason=NULL, moderatedBy=NULL, moderatedAtUtc=NULL WHERE id IN (@Headphones,@Keyboard,@Coffee);
    UPDATE Product SET moderationStatus=N'Hidden', moderationReason=N'DEMO HIDDEN: listing requires manual verification.', moderatedAtUtc=SYSUTCDATETIME() WHERE id=@Camera;

    IF NOT EXISTS (SELECT 1 FROM OrderTable WHERE buyerId=@BuyerA AND totalPrice=129.99 AND status=N'Delivered') INSERT INTO OrderTable(buyerId,addressId,orderDate,totalPrice,status) VALUES(@BuyerA,@AddressA,DATEADD(day,-10,GETDATE()),129.99,N'Delivered');
    IF NOT EXISTS (SELECT 1 FROM OrderTable WHERE buyerId=@BuyerA AND totalPrice=89.50 AND status=N'Pending') INSERT INTO OrderTable(buyerId,addressId,orderDate,totalPrice,status) VALUES(@BuyerA,@AddressA,DATEADD(day,-2,GETDATE()),89.50,N'Pending');
    IF NOT EXISTS (SELECT 1 FROM OrderTable WHERE buyerId=@BuyerB AND totalPrice=59.00 AND status=N'Completed') INSERT INTO OrderTable(buyerId,addressId,orderDate,totalPrice,status) VALUES(@BuyerB,@AddressB,DATEADD(day,-20,GETDATE()),59.00,N'Completed');
    IF NOT EXISTS (SELECT 1 FROM OrderTable WHERE buyerId=@BuyerB AND totalPrice=220.00 AND status=N'Processing') INSERT INTO OrderTable(buyerId,addressId,orderDate,totalPrice,status) VALUES(@BuyerB,@AddressB,DATEADD(day,-4,GETDATE()),220.00,N'Processing');
    DECLARE @Delivered int=(SELECT id FROM OrderTable WHERE buyerId=@BuyerA AND totalPrice=129.99 AND status=N'Delivered');
    DECLARE @Pending int=(SELECT id FROM OrderTable WHERE buyerId=@BuyerA AND totalPrice=89.50 AND status=N'Pending');
    DECLARE @Completed int=(SELECT id FROM OrderTable WHERE buyerId=@BuyerB AND totalPrice=59.00 AND status=N'Completed');
    DECLARE @Processing int=(SELECT id FROM OrderTable WHERE buyerId=@BuyerB AND totalPrice=220.00 AND status=N'Processing');

    IF NOT EXISTS (SELECT 1 FROM OrderItem WHERE orderId=@Delivered AND productId=@Headphones) INSERT INTO OrderItem(orderId,productId,quantity,unitPrice) VALUES(@Delivered,@Headphones,1,129.99);
    IF NOT EXISTS (SELECT 1 FROM OrderItem WHERE orderId=@Pending AND productId=@Keyboard) INSERT INTO OrderItem(orderId,productId,quantity,unitPrice) VALUES(@Pending,@Keyboard,1,89.50);
    IF NOT EXISTS (SELECT 1 FROM OrderItem WHERE orderId=@Completed AND productId=@Coffee) INSERT INTO OrderItem(orderId,productId,quantity,unitPrice) VALUES(@Completed,@Coffee,1,59.00);
    IF NOT EXISTS (SELECT 1 FROM OrderItem WHERE orderId=@Processing AND productId=@Camera) INSERT INTO OrderItem(orderId,productId,quantity,unitPrice) VALUES(@Processing,@Camera,1,220.00);

    IF NOT EXISTS (SELECT 1 FROM Payment WHERE orderId=@Delivered) INSERT INTO Payment(orderId,userId,amount,method,status,paidAt) VALUES(@Delivered,@BuyerA,129.99,N'PayPal',N'Paid',DATEADD(day,-10,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM Payment WHERE orderId=@Pending) INSERT INTO Payment(orderId,userId,amount,method,status,paidAt) VALUES(@Pending,@BuyerA,89.50,N'COD',N'Pending',NULL);
    IF NOT EXISTS (SELECT 1 FROM Payment WHERE orderId=@Completed) INSERT INTO Payment(orderId,userId,amount,method,status,paidAt) VALUES(@Completed,@BuyerB,59.00,N'Card',N'Paid',DATEADD(day,-20,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM Payment WHERE orderId=@Processing) INSERT INTO Payment(orderId,userId,amount,method,status,paidAt) VALUES(@Processing,@BuyerB,220.00,N'Card',N'Failed',NULL);

    IF NOT EXISTS (SELECT 1 FROM ShippingInfo WHERE orderId=@Delivered) INSERT INTO ShippingInfo(orderId,carrier,trackingNumber,status,estimatedArrival) VALUES(@Delivered,N'Demo Express',N'DEMO-DELIVERED-001',N'Delivered',DATEADD(day,-7,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM ShippingInfo WHERE orderId=@Pending) INSERT INTO ShippingInfo(orderId,carrier,trackingNumber,status,estimatedArrival) VALUES(@Pending,N'Demo Express',N'DEMO-PENDING-001',N'Preparing',DATEADD(day,3,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM ShippingInfo WHERE orderId=@Completed) INSERT INTO ShippingInfo(orderId,carrier,trackingNumber,status,estimatedArrival) VALUES(@Completed,N'VN Demo Post',N'DEMO-COMPLETE-001',N'Delivered',DATEADD(day,-17,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM ShippingInfo WHERE orderId=@Processing) INSERT INTO ShippingInfo(orderId,carrier,trackingNumber,status,estimatedArrival) VALUES(@Processing,N'VN Demo Post',N'DEMO-PROCESS-001',N'AwaitingPayment',DATEADD(day,5,GETDATE()));

    IF NOT EXISTS (SELECT 1 FROM ReturnRequest WHERE orderId=@Delivered AND reason=N'DEMO RETURN PENDING: Headphones have intermittent audio.') INSERT INTO ReturnRequest(orderId,userId,reason,status,createdAt) VALUES(@Delivered,@BuyerA,N'DEMO RETURN PENDING: Headphones have intermittent audio.',N'Pending',DATEADD(day,-1,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM ReturnRequest WHERE orderId=@Completed AND reason=N'DEMO RETURN APPROVED: Coffee set arrived chipped.') INSERT INTO ReturnRequest(orderId,userId,reason,status,createdAt) VALUES(@Completed,@BuyerB,N'DEMO RETURN APPROVED: Coffee set arrived chipped.',N'Approved',DATEADD(day,-12,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM ReturnRequest WHERE orderId=@Processing AND reason=N'DEMO RETURN REJECTED: Order has not been delivered.') INSERT INTO ReturnRequest(orderId,userId,reason,status,createdAt) VALUES(@Processing,@BuyerB,N'DEMO RETURN REJECTED: Order has not been delivered.',N'Rejected',DATEADD(day,-3,GETDATE()));

    IF NOT EXISTS (SELECT 1 FROM Dispute WHERE orderId=@Delivered AND description=N'DEMO DISPUTE OPEN: Item differs from listing description.') INSERT INTO Dispute(orderId,raisedBy,description,status,resolution) VALUES(@Delivered,@BuyerA,N'DEMO DISPUTE OPEN: Item differs from listing description.',N'Open',NULL);
    IF NOT EXISTS (SELECT 1 FROM Dispute WHERE orderId=@Completed AND description=N'DEMO DISPUTE RESOLVED: Damaged item claim.') INSERT INTO Dispute(orderId,raisedBy,description,status,resolution) VALUES(@Completed,@BuyerB,N'DEMO DISPUTE RESOLVED: Damaged item claim.',N'Resolved',N'Buyer refund requested and seller notified.');
    IF NOT EXISTS (SELECT 1 FROM Dispute WHERE orderId=@Processing AND description=N'DEMO DISPUTE REJECTED: Premature delivery complaint.') INSERT INTO Dispute(orderId,raisedBy,description,status,resolution) VALUES(@Processing,@BuyerB,N'DEMO DISPUTE REJECTED: Premature delivery complaint.',N'Rejected',N'Order is still processing; no action.');
    UPDATE Dispute SET status=N'Open',resolution=NULL,assignedTo=NULL,assignedAtUtc=NULL,reviewStartedAtUtc=NULL,resolvedBy=NULL,resolvedAtUtc=NULL WHERE description=N'DEMO DISPUTE OPEN: Item differs from listing description.';

    IF NOT EXISTS (SELECT 1 FROM Review WHERE productId=@Headphones AND comment=N'DEMO REVIEW: Excellent sound quality.') INSERT INTO Review(productId,reviewerId,rating,comment,createdAt) VALUES(@Headphones,@BuyerA,5,N'DEMO REVIEW: Excellent sound quality.',DATEADD(day,-8,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM Review WHERE productId=@Coffee AND comment=N'DEMO REVIEW: Product is acceptable overall.') INSERT INTO Review(productId,reviewerId,rating,comment,createdAt) VALUES(@Coffee,@BuyerB,3,N'DEMO REVIEW: Product is acceptable overall.',DATEADD(day,-18,GETDATE()));
    IF NOT EXISTS (SELECT 1 FROM Review WHERE productId=@Keyboard AND comment=N'DEMO REVIEW: Keys stopped responding.') INSERT INTO Review(productId,reviewerId,rating,comment,createdAt) VALUES(@Keyboard,@BuyerA,1,N'DEMO REVIEW: Keys stopped responding.',DATEADD(day,-1,GETDATE()));
    UPDATE Review SET moderationStatus=N'Visible', moderationReason=NULL, moderatedBy=NULL, moderatedAtUtc=NULL WHERE comment IN (N'DEMO REVIEW: Excellent sound quality.',N'DEMO REVIEW: Keys stopped responding.');
    UPDATE Review SET moderationStatus=N'Hidden', moderationReason=N'DEMO HIDDEN REVIEW: awaiting policy review.', moderatedAtUtc=SYSUTCDATETIME() WHERE comment=N'DEMO REVIEW: Product is acceptable overall.';

    IF NOT EXISTS (SELECT 1 FROM Feedback WHERE sellerId=@SellerA) INSERT INTO Feedback(sellerId,averageRating,totalReviews,positiveRate) VALUES(@SellerA,4.35,46,91.30);
    IF NOT EXISTS (SELECT 1 FROM Feedback WHERE sellerId=@SellerB) INSERT INTO Feedback(sellerId,averageRating,totalReviews,positiveRate) VALUES(@SellerB,3.80,20,75.00);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
