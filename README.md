
# DotNetAssignment2

## Getting Started

### Prerequisites

- **.NET 8 SDK** or higher
- **Entity Framework Core Tools** (If not installed, use the following command)

   ```bash
   dotnet tool install --global dotnet-ef
   ```

### Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/ChengEn-Tsai/NeuralNetworkTrainer.git
   ```

2. **Navigate into the project directory**:

   ```bash
   cd NeuralNetworkTrainer
   ```

3. **Restore dependencies**:

   ```bash
   dotnet restore
   ```

4. **Reset and Rebuild the Database** (Optional, if you need to reset the database):

   1. **Delete existing migrations**:  
      If there are existing migrations, delete the `Migrations` folder:

      ```bash
      rm -r Migrations
      ```

   2. **Drop the existing database**:

      ```bash
      dotnet ef database drop
      ```

      This will delete the entire database.

   3. **Add a new initial migration**:

      ```bash
      dotnet ef migrations add InitialCreate
      ```

   4. **Update the database**:

      ```bash
      dotnet ef database update
      ```

      This will recreate the database with the latest schema.

5. **Run the project**:

   ```bash
   dotnet watch run
   ```

   This will launch the application and automatically watch for changes.
