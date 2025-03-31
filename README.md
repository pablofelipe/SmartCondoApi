# smartCondo

## 📌 Overview
This project is a condominium administration system that simplifies user management, communication between residents and administrators, and hierarchical permission control. The system was developed with a focus on security, scalability, and usability.

## ✨ Key Features

### 👥 User and Permission Management
- **User hierarchy**:
  - System Administrator (global access)
  - Manager (condominium administrator)
  - Council Members (condominium board)
  - Residents (regular users)
  
- **Role transitions**:
  - Promote administrator to manager
  - Change committee member to council member
  
- **Access rules**:
  - Users can only register others from the same condominium (with proper permissions)
  - System Administrators can register any user in any condominium
  - Same logic applies to message sending

### 📩 Messaging System
- Messages to the council are broadcast to all council members ("council" group)
- Real-time message delivery (including mobile devices)
- Filtering by user type

### 🔐 Authentication and Security
- Login with encrypted passwords
- Secure logout
- Email verification during registration
- Password recovery ("Forgot password")
- Secure profile updates

### 🚗 Condominium Management
- Vehicle registration and management by residents
- Access control and permissions

## 🛠 Technologies and Infrastructure
- **Docker**: Containerization for easy deployment
- **AWS**: Cloud hosting (using EC2, RDS, S3)
- **Backend**: DotNet
- **Frontend**: React
- **Database**: PostgreSQL
- **Authentication**: JWT

## 🚀 How to Run the Project

### Prerequisites
- Docker installed
- Docker Compose
- Configured AWS CLI (for deployment)

### Local Execution
```bash
# Clone the repository
git clone http://github.com/pablofelipe/SmartCondoApi

# Navigate to project directory
cd SmartCondoApi

# Build and start containers
docker-compose up --build

# The application will be available at:
http://localhost:3000 (frontend)
http://localhost:5254/api/v1 (backend)


## 🤝 Contribution
Contributions are welcome! Please open an issue or submit a pull request following our contribution guidelines.

```

You can save this content as `README.md` in your project's root directory. The file follows standard Markdown formatting and includes all the key information from the Portuguese version while maintaining proper technical documentation structure.