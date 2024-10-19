
# DotNetAssignment2

## Getting Started

### Prerequisites

- **.NET 8 SDK** or higher

### Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/ChengEn-Tsai/NeuralNetworkTrainer.git
   ```

2. **Navigate into the project directory**:

   ```bash
   cd NeuralNetworkTrainer
   ```

3. **Install the `dotnet-ef` tool globally**:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. **Restore dependencies**:

   ```bash
   dotnet restore
   ```

5. **Run the project**:

   ```bash
   dotnet watch run
   ```

   This will launch the application and automatically watch for changes.

---

### Database Management

If you need to drop and recreate the database:

1. **Drop the database**:

   ```bash
   dotnet ef database drop
   ```

2. **Apply migrations and recreate the database**:

   ```bash
   dotnet ef database update
   ```

---

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
