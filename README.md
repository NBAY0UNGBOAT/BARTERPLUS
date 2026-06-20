# BarterPOS - Point of Sale System

A modern WPF-based Point of Sale (POS) system for retail operations, featuring a comprehensive sales interface with product management, customer tracking, and multiple payment options.

## Features

- **Sales Dashboard**: Real-time display of sales information including amount due, total items, and savings
- **Product Management**: Add, remove, and manage products in the current transaction
- **Customer Management**: Track customer information, loyalty points, and credit limits
- **Barcode Scanner Integration**: Quick product entry via barcode scanning
- **Multiple Payment Methods**: Support for cash, card, and EPS payments
- **Cashier & Bagger Management**: Track staff involvement in transactions
- **Function Key Interface**: Quick access to common operations via F1-F12 buttons

## Project Structure

```
BarterPOS/
├── Models/
│   ├── Product.cs          # Product entity model
│   ├── Customer.cs         # Customer entity model
│   └── Sale.cs            # Sale transaction model
├── ViewModels/
│   └── SalesViewModel.cs   # MVVM ViewModel for sales screen
├── Views/
│   └── (Additional views for future expansion)
├── MainWindow.xaml         # Main POS interface
├── MainWindow.xaml.cs      # Code-behind for main window
├── App.xaml                # Application resources and styles
└── App.xaml.cs            # Application entry point
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension
- Windows 10 or later

### Building

```bash
dotnet build
```

### Running

```bash
dotnet run
```

Or press F5 in Visual Studio to debug.

## Architecture

The application uses the **MVVM (Model-View-ViewModel)** pattern:

- **Models**: Contain business logic and data structures (Product, Customer, Sale)
- **ViewModels**: Handle presentation logic and state management (SalesViewModel)
- **Views**: XAML-based UI that binds to ViewModels

## Key Components

### MainWindow
The main POS interface with three sections:
- **Left Panel**: Sales summary and customer information
- **Right Panel**: Products table and barcode scanner
- **Function Bar**: F1-F12 buttons for operations

### SalesViewModel
Manages:
- Current sale state
- Product addition/removal
- Barcode processing
- Transaction initialization

## Customization

### Colors
Edit the color palette in `App.xaml` under `Application.Resources`:
```xaml
<Color x:Key="PrimaryBlue">#2D5AA6</Color>
<Color x:Key="AccentGreen">#6FCC3A</Color>
<Color x:Key="AccentOrange">#FF9800</Color>
```

### Styles
Modify button styles and other elements in the global style resources section.

## Future Enhancements

- Database integration for product catalog
- Receipt printing functionality
- Inventory management system
- Advanced reporting and analytics
- User authentication and role-based access
- Touch-screen optimized interface

## License

This project is provided as-is for educational and commercial use.

## Support

For questions or issues, please contact the development team.
