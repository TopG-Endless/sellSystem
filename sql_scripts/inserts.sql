INSERT INTO Companies (companyName) VALUES 
('Tech Solutions'), 
('Green Enterprises'), 
('Smart Innovations'), 
('Health Corp'), 
('Eco Products'), 
('Fintech Services'), 
('Design Studio'), 
('Logistics Ltd'), 
('Construction Co'), 
('Foodies Inc');

INSERT INTO Employees (employeeName, companyId) VALUES 
('Alice Johnson', 1), 
('Bob Smith', 2), 
('Charlie Brown', 3), 
('Diana Prince', 4), 
('Ethan Hunt', 5);

INSERT INTO Articles (articleName, price, companyId) VALUES 
('Laptop', 999.99, 1), 
('Smartphone', 599.99, 1), 
('Tablet', 399.99, 2), 
('Headphones', 199.99, 3), 
('Charger', 29.99, 3), 
('Printer', 149.99, 4), 
('Desk', 89.99, 5), 
('Chair', 79.99, 5), 
('Monitor', 249.99, 6), 
('Webcam', 59.99, 7);

INSERT INTO Orders (orderName, employeeId, totalValue, status) VALUES 
('Order 1', 21, 1599.98, 'Completed'), 
('Order 2', 22, 399.99, 'Pending'), 
('Order 3', 23, 199.99, 'Pending'), 
('Order 4', 24, 89.99, 'Completed'), 
('Order 5', 25, 169.98, 'Pending');

INSERT INTO OrderDetails (articleId, orderId, quantity) VALUES 
(1, 6, 1), 
(2, 7, 1), 
(3, 8, 1), 
(4, 9, 1), 
(5, 10, 1);

INSERT INTO Invoice (orderId, status, invoiceETA) VALUES 
(6, 'Paid', '2024-10-25'), 
(7, 'Pending', '2024-11-01'), 
(8, 'Pending', '2024-11-05'), 
(9, 'Paid', '2024-10-20'), 
(10, 'Pending', '2024-11-10');

SELECT * FROM Companies;

SELECT Employees.employeeName, Companies.companyName 
FROM Employees 
JOIN Companies ON Employees.companyID = Companies.id;

SELECT o.orderName, e.employeeName, o.totalValue, o.status 
FROM Orders o 
JOIN Employees e ON o.employeeId = e.employeeId;

SELECT i.invoiceId, o.orderId, SUM(od.quantity * a.price) AS totalValue 
FROM Invoice i 
JOIN Orders o ON i.orderId = o.orderId 
JOIN OrderDetails od ON o.orderId = od.orderId 
JOIN Articles a ON od.articleId = a.articleId 
GROUP BY i.invoiceId, o.orderId;