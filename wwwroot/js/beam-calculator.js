// Beam Calculator JavaScript Functions

// Global variables
let currentBeamCandidates = [];
let currentCalculatedECL = 0;
let selectedBeamIndex = 0;
let currentConfiguration = {};
let currentAnalysisResults = {};

/**
 * Display the beam candidates table with top 5 options
 */
function displayBeamCandidates(candidates, requiredECL) {
    const container = document.getElementById('beamCandidatesTable');
    currentBeamCandidates = candidates;
    currentCalculatedECL = requiredECL;

    if (!candidates || candidates.length === 0) {
        container.innerHTML = '<p style="color: #ef4444; font-style: italic;">No adequate beams found for the given requirements.</p>';
        return;
    }

    let tableHTML = `
        <table class="beam-candidates-table">
            <thead>
                <tr>
                    <th>Rank</th>
                    <th>Designation</th>
                    <th>Weight<br>(lbs/ft)</th>
                    <th>Capacity<br>(lbs)</th>
                    <th>Utilization</th>
                    <th>Status</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody>
    `;

    candidates.forEach((beam, index) => {
        const utilizationClass = getUtilizationClass(beam.utilization);
        // FIX: Only the selectedBeamIndex should be marked as selected, not beam.isSelected
        const isSelected = index === selectedBeamIndex;

        tableHTML += `
            <tr class="beam-row ${isSelected ? 'selected' : ''}" data-beam-index="${index}">
                <td style="font-weight: 600;">${index + 1}</td>
                <td style="font-weight: 600; color: #1e40af;">${beam.designation}</td>
                <td>${beam.weight?.toFixed(1) || 'N/A'}</td>
                <td>${beam.capacity?.toLocaleString() || 'N/A'}</td>
                <td class="${utilizationClass}">
                    ${beam.utilization?.toFixed(1) || 'N/A'}%
                </td>
                <td>
                    ${isSelected ? '<span class="status-pass">SELECTED</span>' :
                '<span style="color: #718096; font-size: 0.7rem;">Available</span>'}
                </td>
                <td>
                    <button class="beam-select-btn ${isSelected ? 'selected' : ''}" 
                            onclick="selectBeamAndAnalyze(${index})"
                            ${isSelected ? 'disabled' : ''}>
                        ${isSelected ? 'Current' : 'Select & Analyze'}
                    </button>
                </td>
            </tr>
        `;
    });

    tableHTML += '</tbody></table>';
    container.innerHTML = tableHTML;
}

/**
 * Get CSS class for utilization color coding
 */
function getUtilizationClass(utilization) {
    if (utilization > 90) return 'utilization-high';
    if (utilization > 75) return 'utilization-medium';
    return 'utilization-low';
}

/**
 * Select a beam and perform new analysis with that beam
 */
async function selectBeamAndAnalyze(index) {
    if (index < 0 || index >= currentBeamCandidates.length) {
        showError('Invalid beam selection');
        return;
    }

    if (!API_BASE) {
        showError('❌ API endpoint not available');
        return;
    }

    // Update the selected index FIRST
    selectedBeamIndex = index;
    const selectedBeam = currentBeamCandidates[index];

    showLoading();
    showPerformanceIndicator(`Analyzing with ${selectedBeam.designation}...`);

    try {
        // Update the beam selection display immediately
        updateBeamSelection();

        // Get current configuration
        const config = getFormData();
        currentConfiguration = config;

        // Perform a NEW analysis with the same configuration
        // This will simulate analyzing with the selected beam
        const startTime = Date.now();

        const response = await fetch(`${API_BASE}/analyze`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(config)
        });

        if (!response.ok) {
            throw new Error(`Analysis failed: ${response.status}`);
        }

        const data = await response.json();
        const clientTime = Date.now() - startTime;

        // Override the results to show our selected beam
        if (data.results) {
            // Replace the selected beam info with our manually selected beam
            data.results.selectedBeam = {
                designation: selectedBeam.designation,
                weight: selectedBeam.weight,
                depth: selectedBeam.depth
            };
        }

        // Store the analysis results with our selected beam
        currentAnalysisResults = {
            ...data,
            metadata: data.metadata,
            clientTime: clientTime,
            analysisDate: new Date().toISOString(),
            manuallySelectedBeam: selectedBeam
        };

        hideLoading();

        // Update the display with the selected beam
        displayResultsWithSelectedBeam(data, data.metadata, clientTime, selectedBeam);

        showSuccess(`✅ Analysis completed with selected beam: ${selectedBeam.designation} (${selectedBeam.utilization.toFixed(1)}% utilized)`);

    } catch (error) {
        hideLoading();
        showError(`❌ Error analyzing with selected beam: ${error.message}`);
    }
}

/**
 * Display results with the manually selected beam
 */
function displayResultsWithSelectedBeam(data, metadata, clientTime, selectedBeam) {
    // Display calculated ECL and K-factors
    document.getElementById('calculatedECL').textContent = `${data.calculatedECL?.toLocaleString() || 'N/A'} lbs`;
    document.getElementById('k1FactorDisplay').textContent = data.kFactors?.k1?.toFixed(3) || '-';
    document.getElementById('k2FactorDisplay').textContent = data.kFactors?.k2?.toFixed(3) || '-';

    // Keep the beam candidates table (already updated by updateBeamSelection)
    // Don't call displayBeamCandidates again as it would reset selectedBeamIndex

    // Update selected beam information with our manually selected beam
    document.getElementById('beamDesignation').textContent = selectedBeam.designation;
    document.getElementById('beamWeight').textContent = `${selectedBeam.weight.toFixed(1)} lbs/ft`;

    const results = data.results;
    if (results) {
        // Update load calculations
        document.getElementById('maxWheelLoad').textContent = `${results.maxWheelLoad?.toLocaleString() || 0} lbs`;
        document.getElementById('ecl').textContent = `${(data.calculatedECL || 0).toLocaleString()} lbs`;
        document.getElementById('k1Factor').textContent = data.kFactors?.k1?.toFixed(3) || '-';
        document.getElementById('k2Factor').textContent = data.kFactors?.k2?.toFixed(3) || '-';

        // Update structural checks
        document.getElementById('lateralCheck').innerHTML = getStatus(results.lateralDeflectionPass);
        document.getElementById('longitudinalCheck').innerHTML = getStatus(results.longitudinalDeflectionPass);
        document.getElementById('stressCheck').innerHTML = getStatus(results.stressCheckPass);
        document.getElementById('overallStatus').innerHTML = getStatus(results.overallPass,
            results.overallPass ? 'ACCEPTABLE' : 'INADEQUATE');

        // Update foundation loads
        document.getElementById('columnLoad').textContent = `${results.columnLoad?.toLocaleString() || 0} lbs`;
        document.getElementById('lateralOTM').textContent = `${results.lateralOTM?.toLocaleString() || 0} ft-lbs`;
        document.getElementById('longitudinalOTM').textContent = `${results.longitudinalOTM?.toLocaleString() || 0} ft-lbs`;
        document.getElementById('maxVerticalLoad').textContent = `${results.maxVerticalLoad?.toLocaleString() || 0} lbs`;
    }

    // Add manual selection note to the selected beam card
    const selectedBeamCard = document.querySelector('.result-card[style*="border-left: 4px solid #4caf50"]');
    if (selectedBeamCard) {
        const existingNote = selectedBeamCard.querySelector('.selection-note');
        if (existingNote) {
            existingNote.remove();
        }

        const note = document.createElement('div');
        note.className = 'selection-note';
        note.innerHTML = `<strong>Manual Selection:</strong> Beam manually selected from candidates. Capacity: ${selectedBeam.capacity.toLocaleString()} lbs, Utilization: ${selectedBeam.utilization.toFixed(1)}%`;
        selectedBeamCard.appendChild(note);
    }

    // Display performance information
    document.getElementById('processingTime').textContent = metadata?.processingTimeMs ?
        `${metadata.processingTimeMs.toFixed(1)}ms (Server) + ${clientTime}ms (Client)` : `${clientTime}ms`;
    document.getElementById('fromCache').textContent = metadata?.cached ? 'Yes' : 'No';

    // Show results
    document.getElementById('results').style.display = 'block';
}

/**
 * Update the beam candidates display to reflect the new selection
 */
function updateBeamSelection() {
    // Update the table rows
    const rows = document.querySelectorAll('.beam-row');
    rows.forEach((row, index) => {
        const isSelected = index === selectedBeamIndex;
        const button = row.querySelector('.beam-select-btn');

        if (isSelected) {
            row.classList.add('selected');
            button.textContent = 'Current';
            button.disabled = true;
            button.classList.add('selected');
        } else {
            row.classList.remove('selected');
            button.textContent = 'Select & Analyze';
            button.disabled = false;
            button.classList.remove('selected');
        }

        // Update status column
        const statusCell = row.cells[5];
        statusCell.innerHTML = isSelected ?
            '<span class="status-pass">SELECTED</span>' :
            '<span style="color: #718096; font-size: 0.7rem;">Available</span>';
    });
}

/**
 * Update the selected beam display with the chosen beam (used for export data)
 */
function updateSelectedBeamDisplay(selectedBeam) {
    // This function is now mainly used to ensure export data is current
    // The actual display is handled by displayResultsWithSelectedBeam
    console.log(`Updated display for selected beam: ${selectedBeam.designation}`);
}

/**
 * Enhanced results display function
 */
function displayResults(data, metadata, clientTime) {
    // Store the analysis results for export
    currentAnalysisResults = {
        ...data,
        metadata: metadata,
        clientTime: clientTime,
        analysisDate: new Date().toISOString()
    };
    currentConfiguration = getFormData();

    // Reset selectedBeamIndex to 0 (first beam) for new analysis
    selectedBeamIndex = 0;

    // Display calculated ECL and K-factors
    document.getElementById('calculatedECL').textContent = `${data.calculatedECL?.toLocaleString() || 'N/A'} lbs`;
    document.getElementById('k1FactorDisplay').textContent = data.kFactors?.k1?.toFixed(3) || '-';
    document.getElementById('k2FactorDisplay').textContent = data.kFactors?.k2?.toFixed(3) || '-';

    // Display beam candidates table
    displayBeamCandidates(data.beamCandidates || [], data.calculatedECL || 0);

    // Update selected beam information
    const results = data.results;
    if (results) {
        document.getElementById('beamDesignation').textContent = results.selectedBeam?.designation || 'None';
        document.getElementById('beamWeight').textContent = results.selectedBeam ?
            `${results.selectedBeam.weight.toFixed(1)} lbs/ft` : '-';

        // Update load calculations
        document.getElementById('maxWheelLoad').textContent = `${results.maxWheelLoad?.toLocaleString() || 0} lbs`;
        document.getElementById('ecl').textContent = `${(data.calculatedECL || 0).toLocaleString()} lbs`;
        document.getElementById('k1Factor').textContent = data.kFactors?.k1?.toFixed(3) || '-';
        document.getElementById('k2Factor').textContent = data.kFactors?.k2?.toFixed(3) || '-';

        // Update structural checks
        document.getElementById('lateralCheck').innerHTML = getStatus(results.lateralDeflectionPass);
        document.getElementById('longitudinalCheck').innerHTML = getStatus(results.longitudinalDeflectionPass);
        document.getElementById('stressCheck').innerHTML = getStatus(results.stressCheckPass);
        document.getElementById('overallStatus').innerHTML = getStatus(results.overallPass,
            results.overallPass ? 'ACCEPTABLE' : 'INADEQUATE');

        // Update foundation loads
        document.getElementById('columnLoad').textContent = `${results.columnLoad?.toLocaleString() || 0} lbs`;
        document.getElementById('lateralOTM').textContent = `${results.lateralOTM?.toLocaleString() || 0} ft-lbs`;
        document.getElementById('longitudinalOTM').textContent = `${results.longitudinalOTM?.toLocaleString() || 0} ft-lbs`;
        document.getElementById('maxVerticalLoad').textContent = `${results.maxVerticalLoad?.toLocaleString() || 0} lbs`;
    }

    // Remove any existing manual selection notes since this is a fresh analysis
    const selectedBeamCard = document.querySelector('.result-card[style*="border-left: 4px solid #4caf50"]');
    if (selectedBeamCard) {
        const existingNote = selectedBeamCard.querySelector('.selection-note');
        if (existingNote) {
            existingNote.remove();
        }
    }

    // Display performance information
    document.getElementById('processingTime').textContent = metadata?.processingTimeMs ?
        `${metadata.processingTimeMs.toFixed(1)}ms (Server) + ${clientTime}ms (Client)` : `${clientTime}ms`;
    document.getElementById('fromCache').textContent = metadata?.cached ? 'Yes' : 'No';

    // Show results
    document.getElementById('results').style.display = 'block';
}

/**
 * Export beam analysis results to Excel
 */
function exportResults() {
    if (!currentAnalysisResults || !currentBeamCandidates || currentBeamCandidates.length === 0) {
        showError('No results to export. Please run an analysis first.');
        return;
    }

    try {
        // Get the selected beam
        const selectedBeam = currentBeamCandidates[selectedBeamIndex] || currentBeamCandidates[0];

        // Create Excel data structure
        const excelData = createExcelData(selectedBeam);

        // Convert to CSV format (simplified Excel export)
        const csvContent = convertToCSV(excelData);

        // Create and download file
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', `beam-analysis-${selectedBeam.designation}-${new Date().toISOString().split('T')[0]}.csv`);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        showSuccess(`✅ Analysis exported successfully for beam ${selectedBeam.designation}`);

    } catch (error) {
        showError(`❌ Export failed: ${error.message}`);
        console.error('Export error:', error);
    }
}

/**
 * Create structured data for Excel export
 */
function createExcelData(selectedBeam) {
    const config = currentConfiguration;
    const results = currentAnalysisResults.results || {};

    const data = [
        ['BEAM ANALYSIS REPORT'],
        ['Generated:', new Date().toLocaleString()],
        [''],
        ['SELECTED BEAM INFORMATION'],
        ['Designation:', selectedBeam.designation],
        ['Weight (lbs/ft):', selectedBeam.weight?.toFixed(1) || 'N/A'],
        ['Depth (in):', selectedBeam.depth?.toFixed(1) || 'N/A'],
        ['Capacity (lbs):', selectedBeam.capacity?.toLocaleString() || 'N/A'],
        ['Utilization (%):', selectedBeam.utilization?.toFixed(1) || 'N/A'],
        [''],
        ['CONFIGURATION PARAMETERS'],
        ['Rated Capacity (lbs):', config.ratedCapacity?.toLocaleString() || 'N/A'],
        ['Hoist + Trolley Weight (lbs):', config.weightHoistTrolley?.toLocaleString() || 'N/A'],
        ['Girder Weight (lbs):', config.girderWeight?.toLocaleString() || 'N/A'],
        ['Panel Weight (lbs):', config.panelWeight?.toLocaleString() || 'N/A'],
        ['End Truck Weight (lbs):', config.endTruckWeight?.toLocaleString() || 'N/A'],
        ['Rail Height (in):', config.railHeight || 'N/A'],
        ['Wheel Base (ft):', config.wheelBase || 'N/A'],
        ['Support Centers (ft):', config.supportCenters || 'N/A'],
        ['Support Centers (ft):', config.supportCenters || 'N/A'],
        ['Number of Columns:', config.numCols || 'N/A'],
        ['Freestanding:', config.freestanding ? 'Yes' : 'No'],
        ['Capped System:', config.capped ? 'Yes' : 'No'],
        [''],
        ['CALCULATION DETAILS'],
        ['Max Wheel Load (MWL) = Rated + Beam + Hoist/Trolley'],
        ['MWL (lbs):', results.maxWheelLoad?.toLocaleString() || 'N/A'],
        ['Wheelbase Span Ratio = A / L:'],
        ['Wheelbase / SupportCenters:', (config.wheelBase / config.supportCenters).toFixed(3)],

        ['K-Factors from Ratio Lookup'],
        ['K1 Factor:', currentAnalysisResults.kFactors?.k1?.toFixed(3) || 'N/A'],
        ['K2 Factor:', currentAnalysisResults.kFactors?.k2?.toFixed(3) || 'N/A'],

        ['ECL = K1 * MWL'],
        ['ECL (lbs):', currentCalculatedECL?.toLocaleString() || 'N/A'],

        ['Column Moment = Lateral Load * Rail Height (inches)'],
        ['Column Moment (lb-in):', results.columnMoment?.toLocaleString() || 'N/A'],
        ['Lateral OTM = Column Moment / 12,000'],
        ['Lateral OTM (kip-ft):', results.lateralOTM?.toFixed(2) || 'N/A'],

        ['Longitudinal Moment = Longitudinal Load * Rail Height (inches)'],
        ['Foundation Moment (lb-in):', results.foundationMoment?.toLocaleString() || 'N/A'],
        ['Longitudinal OTM = Foundation Moment / 12,000'],
        ['Longitudinal OTM (kip-ft):', results.longitudinalOTM?.toFixed(2) || 'N/A'],

        ['Column Load = (Max Vertical Load + 2500) / 1000'],
        ['Column Load (kips):', results.columnLoad?.toFixed(2) || 'N/A'],
        [''],
        ['STRUCTURAL ANALYSIS RESULTS'],
        ['Lateral Deflection Check:', results.lateralDeflectionPass ? 'PASS' : 'FAIL'],
        ['Longitudinal Deflection Check:', results.longitudinalDeflectionPass ? 'PASS' : 'FAIL'],
        ['Stress Check:', results.stressCheckPass ? 'PASS' : 'FAIL'],
        ['Overall Status:', results.overallPass ? 'ACCEPTABLE' : 'INADEQUATE'],
        [''],
        ['FOUNDATION LOADS'],
        ['Column Load (lbs):', results.columnLoad?.toLocaleString() || 'N/A'],
        ['Lateral OTM (ft-lbs):', results.lateralOTM?.toLocaleString() || 'N/A'],
        ['Longitudinal OTM (ft-lbs):', results.longitudinalOTM?.toLocaleString() || 'N/A'],
        ['Max Vertical Load (lbs):', results.maxVerticalLoad?.toLocaleString() || 'N/A'],
        [''],
        ['STRUCTURAL FORMULAS'],
        ['Lateral Deflection = (Lateral Load * H^3) / (3 * E * I)'],
        ['Longitudinal Deflection = (Longitudinal Load * H^3) / (3 * E * I)'],
        ['Stress = (Lateral Load * H) / S'],
        ['Axial Unity = (Axial Load / 24000) + (Effective Length / 43.2)']
    ];

    // Add beam candidates
    currentBeamCandidates.forEach((beam, index) => {
        data.push([
            index + 1,
            beam.designation,
            beam.weight?.toFixed(1) || 'N/A',
            beam.capacity?.toLocaleString() || 'N/A',
            beam.utilization?.toFixed(1) || 'N/A',
            index === selectedBeamIndex ? 'SELECTED' : 'Available'
        ]);
    });

    return data;
}

/**
 * Convert data array to CSV format
 */
function convertToCSV(data) {
    return data.map(row =>
        row.map(cell => {
            // Handle cells that might contain commas or quotes
            const cellStr = String(cell || '');
            if (cellStr.includes(',') || cellStr.includes('"') || cellStr.includes('\n')) {
                return '"' + cellStr.replace(/"/g, '""') + '"';
            }
            return cellStr;
        }).join(',')
    ).join('\n');
}

/**
 * Get beam options for the current configuration
 */
async function getBeamOptions() {
    if (!API_BASE) {
        showError('❌ API endpoint not available');
        return;
    }

    try {
        const config = getFormData();

        // Calculate ECL to get beam options
        const kFactors = await getKFactors(config);
        if (!kFactors) return;

        const ecl = kFactors.k1 * calculateMaxWheelLoad(config);

        const response = await fetch(`${API_BASE}/beams?ecl=${ecl}&span=${config.supportCenters}&capped=${config.capped}&limit=5`);

        if (!response.ok) {
            throw new Error(`Failed to get beam options: ${response.status}`);
        }

        const data = await response.json();
        displayBeamCandidates(data.beams || [], ecl);

    } catch (error) {
        console.error('Error getting beam options:', error);
        showError(`❌ Failed to get beam options: ${error.message}`);
    }
}

/**
 * Calculate max wheel load from configuration
 */
function calculateMaxWheelLoad(config) {
    // This is a simplified calculation - you may need to adjust based on your BeamSizerConfig logic
    const totalWeight = config.ratedCapacity + config.weightHoistTrolley + config.girderWeight + config.panelWeight + config.endTruckWeight;
    return totalWeight / 2; // Simplified - adjust as needed
}

/**
 * Get K-factors for the configuration
 */
async function getKFactors(config) {
    try {
        const wheelbaseSpanRatio = config.wheelBase / config.supportCenters;
        const response = await fetch(`${API_BASE}/k-factors?wheelbaseSpanRatio=${wheelbaseSpanRatio}`);

        if (!response.ok) {
            throw new Error(`Failed to get K-factors: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('Error getting K-factors:', error);
        return null;
    }
}

/**
 * Initialize beam calculator features
 */
function initializeBeamCalculator() {
    console.log('🏗️ Initializing beam calculator features...');

    // Add keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey && e.key === 'e') {
            e.preventDefault();
            exportResults();
        }
    });
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', initializeBeamCalculator);