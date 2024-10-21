CREATE TABLE Companies (
    id INT AUTO_INCREMENT PRIMARY KEY,
    companyName varchar(25)
);

CREATE TABLE Employees (
    employeeId INT AUTO_INCREMENT PRIMARY KEY,
    employeeName varchar(15) NOT NULL,
    companyId INT,
    FOREIGN KEY (companyId) REFERENCES Companies(id)
);

CREATE TABLE Articules (
    articuleId INT AUTO_INCREMENT  PRIMARY KEY,
    articuleName varchar(20) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    companyId INT,
    FOREIGN KEY (companyId) REFERENCES Companies(id)
);

CREATE TABLE Orders (
    orderId INT AUTO_INCREMENT  PRIMARY KEY,
    orderName varchar(100) NOT NULL,
    employeeId INT,
    totalValue INT,
    status ENUM('Pending', 'Completed') DEFAULT 'Pending',
    FOREIGN KEY (employeeId) REFERENCES Employees(employeeId)        
);

CREATE TABLE OrderDetails (
    orderDetailId INT AUTO_INCREMENT PRIMARY KEY,
    articleId INT,
    orderId INT,
    quantity INT NOT NULL,
    FOREIGN KEY (articleId) REFERENCES Articles(articleId),
    FOREIGN KEY (orderId) REFERENCES Orders(orderId)
);

CREATE TABLE Invoice (
    invoiceId INT AUTO_INCREMENT PRIMARY KEY,
    orderId INT,
    status ENUM('Pending', 'Paid') DEFAULT 'Pending',
    invoiceETA DATE NOT NULL,
    FOREIGN KEY (orderId) REFERENCES Orders(orderId)
);