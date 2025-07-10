# Beam Calculator - Windows Edition

A professional beam sizing and analysis tool for structural engineering applications. This application provides comprehensive beam selection, load analysis, and structural validation for both capped and uncapped beam systems.

## 🏗️ Features

### Core Functionality
- **Automated Beam Selection**: Finds the lightest adequate beam from extensive AISC database
- **Dual System Support**: Both capped (W-shape + Channel) and uncapped beam configurations
- **Advanced Load Analysis**: K-factor calculations, ECL (Equivalent Concentrated Load) determination
- **Structural Validation**: Deflection, stress, and axial unity checks
- **Interactive Beam Selection**: Top 5 candidates with manual selection and re-analysis capability
- **Foundation Load Calculations**: Column loads and overturning moments

### Engineering Features
- **Interpolated Capacity Lookup**: Handles non-standard span lengths with linear interpolation
- **Impact Factor Calculations**: Based on hoist speed or default values
- **Comprehensive Structural Checks**:
  - Lateral deflection (L/450 limit)
  - Longitudinal deflection (L/500 limit)
  - Bending stress (24,000 psi limit)
  - Axial unity check with interaction formulas

### User Interface
- **Modern Web Interface**: Responsive design optimized for Windows environments
- **Real-time Analysis**: Instant feedback and validation
- **Export Capabilities**: Excel/CSV export of complete analysis results
- **Performance Monitoring**: Processing time and cache status display
- **System Status Dashboard**: Platform, memory, and cache monitoring

## 📋 Requirements

### System Requirements
- Windows Server 2022 or Windows 10/11
- .NET 8.0 Runtime
- Minimum 4GB RAM (8GB recommended)
- Network access for multi-user deployment

### Development Requirements
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- ASP.NET Core runtime

## 🚀 Quick Start

### Option 1: Direct Execution
```bash
# Clone the repository
git clone <repository-url>
cd beam-calculator

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

### Option 2: Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up -d

# Access the application
# Local: http://localhost:5265
# Network: http://<your-ip>:5265
```

## 🔧 Configuration

### Basic Configuration Parameters

| Parameter | Description | Units | Typical Range |
|-----------|-------------|-------|---------------|
| **Rated Capacity** | Maximum crane load capacity | lbs | 1,000 - 80,000 |
| **Hoist + Trolley Weight** | Combined weight of hoist and trolley | lbs | 500 - 10,000 |
| **Girder Weight** | Weight of crane girder | lbs | 1,000 - 15,000 |
| **Panel Weight** | Weight of crane panels | lbs | 500 - 5,000 |
| **End Truck Weight** | Weight of end trucks | lbs | 500 - 3,000 |
| **Rail Height** | Height from floor to rail | ft | 8 - 100 |
| **Wheel Base** | Distance between crane wheels | ft | 3 - 50 |
| **Support Centers** | Distance between beam supports | ft | 10 - 150 |
| **Number of Columns** | Columns per side | count | 2+ |

### System Options
- **Freestanding**: Column support condition (checked = freestanding)
- **Capped System**: Use capped beam system (W-shape + Channel combination)
- **Hoist Speed**: For impact factor calculation (ft/min, 0 = default factor)

## 🏗️ Engineering Methodology

### Load Calculations
```
Max Wheel Load (MWL) = (Impact Factor × Rated Capacity) / 2 + 
                       (Hoist/Trolley Weight) / 2 + 
                       (Total Beam Weight) / 4

Equivalent Concentrated Load (ECL) = K1 × MWL

Lateral Load = 0.2 × (Rated Capacity + Hoist/Trolley Weight)
Longitudinal Load = 0.1 × MWL
```

### K-Factor Determination
K-factors are determined from the wheelbase-to-span ratio (A/L) using engineering tables:
- **K1**: Load distribution factor for beam selection
- **K2**: Load distribution factor for secondary calculations
- Interpolated from standard engineering tables for ratios 0.0 to 1.0

### Structural Checks
1. **Lateral Deflection**: δ = (P × H³) / (3 × E × I) ≤ H/450
2. **Longitudinal Deflection**: δ = (P × H³) / (3 × E × I) ≤ H/500
3. **Bending Stress**: σ = (P × H) / S ≤ 24,000 psi
4. **Axial Unity**: (fa/Fa) + (fe/Fe) ≤ 1.0

## 📊 Beam Database

### Uncapped Beams
- Complete AISC W-shape database (W6 through W36)
- Load capacities for spans 10-60 feet
- Properties: weight, moment of inertia, section modulus, etc.

### Capped Beams
- W-shape + Channel combinations
- Enhanced load capacities through composite action
- Optimized for heavy-duty applications
- Custom designations (e.g., "12x40+12x20.7")

## 🔄 API Endpoints

### Analysis Endpoints
- `POST /api/beamsizing/analyze` - Complete beam analysis
- `POST /api/beamsizing/validate` - Configuration validation
- `GET /api/beamsizing/beams` - Get beam options for requirements
- `GET /api/beamsizing/k-factors` - K-factor lookup

### System Endpoints
- `GET /api/beamsizing/health` - System health check
- `GET /api/beamsizing/system-info` - System information
- `POST /api/beamsizing/clear-cache` - Clear analysis cache

## 📱 Usage Guide

### Basic Analysis Workflow
1. **Configure Parameters**: Enter crane specifications and structural requirements
2. **Validate Configuration**: Check parameter validity before analysis
3. **Run Analysis**: Generate beam recommendations and structural analysis
4. **Review Results**: Examine top 5 beam candidates and detailed calculations
5. **Select Alternative**: Manually select different beam and re-analyze if needed
6. **Export Results**: Generate Excel/CSV report for documentation

### Interpreting Results

#### Beam Candidates Table
- **Rank**: Sorted by weight (lightest first)
- **Designation**: Beam size designation
- **Weight**: Weight per linear foot (lbs/ft)
- **Capacity**: Maximum allowable load (lbs)
- **Utilization**: Load utilization percentage
- **Status**: Selection status (SELECTED/Available)

#### Structural Analysis
- **Load Analysis**: Max wheel load, ECL, K-factors
- **Structural Checks**: Pass/fail status for all limit states
- **Foundation Loads**: Column loads and overturning moments

## 🐳 Docker Deployment

### Docker Compose Configuration
```yaml
version: '3.8'
services:
  beam-calculator:
    build: .
    ports:
      - "5265:8080"
      - "7089:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
```

### Network Access
The application automatically detects and displays network access information:
- **Local Access**: `http://localhost:5265`
- **Network Access**: `http://<server-ip>:5265`
- **HTTPS**: `https://<server-ip>:7089`

## 🧪 Development

### Project Structure
```
beam-calculator/
├── src/
│   ├── Controllers/           # API controllers
│   ├── Core/
│   │   ├── Calculations/      # Beam calculation engine
│   │   ├── Data/              # Beam property databases
│   │   └── Models/            # Data models
│   └── Data/Models/           # Entity models
├── wwwroot/                   # Static web files
│   ├── css/                   # Stylesheets
│   ├── js/                    # JavaScript files
│   └── index.html             # Main application page
├── Properties/                # Launch settings
├── Dockerfile                 # Container configuration
├── docker-compose.yml         # Multi-container setup
└── BeamSizer.csproj          # Project file
```

### Key Classes
- **`BeamCalculator`**: Core calculation engine
- **`BeamSizerConfig`**: Immutable configuration structure
- **`DataLoader`**: Beam database and capacity lookup
- **`BeamSizingResults`**: Analysis results container
- **`BeamProperties`**: Beam structural properties

### Adding New Beam Data
1. Update beam property files in `src/Core/Data/BeamData/`
2. Add capacity data to appropriate capacity files
3. Verify data integrity with validation tools
4. Test with known engineering examples

## 🔍 Troubleshooting

### Common Issues

#### "No adequate beams found"
- **Cause**: Required load exceeds available beam capacities
- **Solution**: Consider capped beam system, reduce loads, or increase span

#### "API endpoint not available"
- **Cause**: Backend service not running or network issues
- **Solution**: Verify service status, check firewall settings

#### "Validation failed"
- **Cause**: Invalid input parameters
- **Solution**: Check parameter ranges and engineering limits

### Performance Optimization
- **Enable Caching**: Use cache for repeated calculations
- **Minimize Interpolations**: Use standard span lengths when possible
- **Monitor Memory**: Clear cache periodically for long-running sessions

## 📄 License

This project is proprietary software. See license file for details.

## 🤝 Support

For technical support and engineering questions:
- Review engineering methodology documentation
- Check troubleshooting guide
- Verify input parameters against typical ranges
- Consult structural engineering standards (AISC, ASCE)

## 🔄 Version History

### Current Version: 1.0.0-Windows Edition
- Initial release with complete beam analysis capability
- Support for both capped and uncapped beam systems
- Interactive beam selection with manual override
- Comprehensive structural validation
- Excel export functionality
- Docker containerization support

---

**Note**: This tool is designed for use by qualified structural engineers. All results should be verified by a licensed professional engineer before use in construction or design applications.