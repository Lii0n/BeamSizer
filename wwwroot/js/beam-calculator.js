// Beam Calculator JavaScript Functions

// Global variables
let currentBeamCandidates = [];
let currentCalculatedECL = 0;
let selectedBeamIndex = 0;
let currentConfiguration = {};
let currentAnalysisResults = {};
let currentSavedAnalysisId = null;

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
        <style>
            .beam-candidates-table {
                width: 100%;
                border-collapse: collapse;
                margin-top: 10px;
                font-size: 0.85rem;
            }
            .beam-candidates-table th {
                background: #f7fafc;
                padding: 8px 6px;
                text-align: left;
                border-bottom: 2px solid #e2e8f0;
                font-weight: 600;
                color: #4a5568;
                font-size: 0.75rem;
            }
            .beam-candidates-table td {
                padding: 8px 6px;
                border-bottom: 1px solid #e2e8f0;
            }
            .beam-row {
                transition: background-color 0.2s;
            }
            .beam-row:hover {
                background: #f0fdf4;
            }
            .beam-row.selected {
                background: #eff6ff;
                border-left: 3px solid #3b82f6;
            }
            .beam-select-btn {
                padding: 4px 8px;
                border: 1px solid #d1d5db;
                border-radius: 4px;
                background: #f9fafb;
                color: #374151;
                font-size: 0.7rem;
                cursor: pointer;
                transition: all 0.2s;
            }
            .beam-select-btn:hover {
                background: #4caf50;
                color: white;
                border-color: #4caf50;
            }
            .beam-select-btn.selected {
                background: #3b82f6;
                color: white;
                border-color: #3b82f6;
                cursor: default;
            }
            .utilization-high { color: #dc2626; font-weight: 600; }
            .utilization-medium { color: #ea580c; font-weight: 600; }
            .utilization-low { color: #16a34a; font-weight: 600; }
        </style>
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

        // Clear saved status since this is a new analysis variant
        hideSavedIndicator();
        currentSavedAnalysisId = null;

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
        note.style.cssText = 'margin-top: 10px; padding: 8px; background: #e0f2fe; border-radius: 6px; font-size: 0.8rem; color: #0369a1;';
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
 * Enhanced results display function
 */
function displayResults(data, metadata, clientTime) {
    // Store the analysis results for export and saving
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
 * Save current analysis to database with enhanced metadata
 */
async function saveCurrentAnalysis(projectName, notes = '') {
    if (!currentAnalysisResults || !currentBeamCandidates || currentBeamCandidates.length === 0) {
        throw new Error('No analysis results to save');
    }

    if (!API_BASE) {
        throw new Error('API endpoint not available');
    }

    const selectedBeam = currentBeamCandidates[selectedBeamIndex] || currentBeamCandidates[0];

    // Prepare comprehensive save data
    const saveData = {
        projectName: projectName,
        userId: 1, // Default user ID - you can enhance this with user authentication
        notes: notes,
        configuration: {
            ...currentConfiguration,
            // Add metadata about the analysis
            analysisType: 'beam_sizing',
            selectedBeamIndex: selectedBeamIndex,
            manuallySelected: !!currentAnalysisResults.manuallySelectedBeam,
            originalAnalysisDate: currentAnalysisResults.analysisDate
        },
        // Include the full analysis results
        analysisResults: {
            ...currentAnalysisResults,
            selectedBeamInfo: {
                ...selectedBeam,
                wasManuallySelected: !!currentAnalysisResults.manuallySelectedBeam
            },
            saveMetadata: {
                savedAt: new Date().toISOString(),
                beamCandidatesCount: currentBeamCandidates.length,
                selectedBeamRank: selectedBeamIndex + 1
            }
        }
    };

    const response = await fetch(`${API_BASE}/analyze-and-save`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(saveData)
    });

    if (!response.ok) {
        const errorData = await response.text();
        throw new Error(`Save failed (${response.status}): ${errorData}`);
    }

    return await response.json();
}

/**
 * Export beam analysis results to Excel with enhanced data
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

        // Create filename with timestamp
        const timestamp = new Date().toISOString().split('T')[0];
        const filename = `beam-analysis-${selectedBeam.designation.replace(/\W/g, '_')}-${timestamp}.csv`;

        // Create and download file
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', filename);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        showSuccess(`✅ Analysis exported successfully as ${filename}`);

    } catch (error) {
        showError(`❌ Export failed: ${error.message}`);
        console.error('Export error:', error);
    }
}

/**
 * Create structured data for Excel export with comprehensive information
 */
function createExcelData(selectedBeam) {
    const config = currentConfiguration;
    const results = currentAnalysisResults.results || {};
    const isFromDatabase = currentAnalysisResults.loadedFromSave;
    const wasManuallSelected = !!currentAnalysisResults.manuallySelectedBeam;

    const data = [
        ['BEAM ANALYSIS REPORT'],
        ['Generated:', new Date().toLocaleString()],
        ['Report Type:', isFromDatabase ? 'Loaded from Database' : 'Fresh Analysis'],
        [''],
        ['PROJECT INFORMATION'],
        ['Analysis ID:', currentSavedAnalysisId || 'Not Saved'],
        ['Analysis Date:', currentAnalysisResults.analysisDate ? new Date(currentAnalysisResults.analysisDate).toLocaleString() : 'N/A'],
        [''],
        ['SELECTED BEAM INFORMATION'],
        ['Designation:', selectedBeam.designation],
        ['Weight (lbs/ft):', selectedBeam.weight?.toFixed(1) || 'N/A'],
        ['Depth (in):', selectedBeam.depth?.toFixed(1) || 'N/A'],
        ['Capacity (lbs):', selectedBeam.capacity?.toLocaleString() || 'N/A'],
        ['Utilization (%):', selectedBeam.utilization?.toFixed(1) || 'N/A'],
        ['Selection Method:', wasManuallSelected ? 'Manually Selected' : 'Automatically Selected'],
        ['Candidate Rank:', (selectedBeamIndex + 1)],
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
        ['Hoist Speed (fpm):', config.hoistSpeed || 'N/A'],
        ['Number of Columns:', config.numCols || 'N/A'],
        ['Freestanding:', config.freestanding ? 'Yes' : 'No'],
        ['Capped System:', config.capped ? 'Yes' : 'No'],
        [''],
        ['CALCULATION DETAILS'],
        ['Max Wheel Load Calculation:'],
        ['MWL = (Rated + Hoist/Trolley + Girder + Panel + EndTruck) / 2'],
        ['MWL (lbs):', results.maxWheelLoad?.toLocaleString() || 'N/A'],
        [''],
        ['Wheelbase to Span Ratio:'],
        ['Ratio = Wheelbase / Support Centers'],
        ['Wheelbase/Support Centers:', config.wheelBase && config.supportCenters ? (config.wheelBase / config.supportCenters).toFixed(3) : 'N/A'],
        [''],
        ['K-Factors (from lookup table):'],
        ['K1 Factor:', currentAnalysisResults.kFactors?.k1?.toFixed(3) || 'N/A'],
        ['K2 Factor:', currentAnalysisResults.kFactors?.k2?.toFixed(3) || 'N/A'],
        [''],
        ['Equivalent Concentrated Load:'],
        ['ECL = K1 × MWL'],
        ['ECL (lbs):', currentCalculatedECL?.toLocaleString() || 'N/A'],
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
        ['PERFORMANCE METRICS'],
        ['Processing Time:', currentAnalysisResults.metadata?.processingTimeMs ?
            `${currentAnalysisResults.metadata.processingTimeMs.toFixed(1)}ms (Server)` : 'N/A'],
        ['Client Time:', currentAnalysisResults.clientTime ? `${currentAnalysisResults.clientTime}ms` : 'N/A'],
        ['From Cache:', currentAnalysisResults.metadata?.cached ? 'Yes' : 'No'],
        [''],
        ['BEAM CANDIDATES SUMMARY'],
        ['Rank', 'Designation', 'Weight (lbs/ft)', 'Capacity (lbs)', 'Utilization (%)', 'Status']
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

    // Add formulas reference
    data.push(['']);
    data.push(['STRUCTURAL FORMULAS REFERENCE']);
    data.push(['Lateral Deflection = (Lateral Load × Height³) / (3 × E × I)']);
    data.push(['Longitudinal Deflection = (Longitudinal Load × Height³) / (3 × E × I)']);
    data.push(['Stress = (Lateral Load × Height) / Section Modulus']);
    data.push(['Column Load = (Max Vertical Load + 2500) / 1000']);
    data.push(['Lateral OTM = (Column Moment) / 12,000']);
    data.push(['Longitudinal OTM = (Foundation Moment) / 12,000']);

    return data;
}

/**
 * Convert data array to CSV format with proper escaping
 */
function convertToCSV(data) {
    return data.map(row =>
        row.map(cell => {
            // Handle cells that might contain commas, quotes, or newlines
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
    const totalWeight = config.ratedCapacity + config.weightHoistTrolley + config.girderWeight + config.panelWeight + config.endTruckWeight;
    return totalWeight / 2; // Simplified - adjust as needed based on your calculation logic
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
 * Initialize beam calculator features with enhanced functionality
 */
function initializeBeamCalculator() {
    console.log('🏗️ Initializing beam calculator with save/load features...');

    // Add keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey || e.metaKey) {
            switch (e.key.toLowerCase()) {
                case 's':
                    e.preventDefault();
                    if (typeof showSaveDialog === 'function') {
                        showSaveDialog();
                    }
                    break;
                case 'o':
                    e.preventDefault();
                    if (typeof showLoadDialog === 'function') {
                        showLoadDialog();
                    }
                    break;
                case 'e':
                    e.preventDefault();
                    exportResults();
                    break;
            }
        }
    });

    // Initialize auto-save prompt for long analyses
    let analysisStartTime = null;

    // Hook into analyze function to track timing
    const originalAnalyze = window.analyze;
    if (originalAnalyze) {
        window.analyze = function () {
            analysisStartTime = Date.now();
            return originalAnalyze.apply(this, arguments);
        };
    }

    // Prompt to save after successful long analyses
    const originalShowSuccess = window.showSuccess;
    if (originalShowSuccess) {
        window.showSuccess = function (message) {
            const result = originalShowSuccess.apply(this, arguments);

            if (analysisStartTime && (Date.now() - analysisStartTime) > 5000) {
                setTimeout(() => {
                    if (currentAnalysisResults && !currentSavedAnalysisId) {
                        if (confirm('This analysis took a while to complete. Would you like to save it for future reference?')) {
                            if (typeof showSaveDialog === 'function') {
                                showSaveDialog();
                            }
                        }
                    }
                }, 2000);
            }

            return result;
        };
    }

    console.log('✅ Enhanced beam calculator features initialized');
}

/**
 * Utility function to get status HTML
 */
function getStatus(passed, text = null) {
    const statusText = text || (passed ? 'PASS' : 'FAIL');
    const className = passed ? 'status-pass' : 'status-fail';
    return `<span class="${className}">${statusText}</span>`;
}

/**
 * Update the selected beam display (used for maintaining UI consistency)
 */
function updateSelectedBeamDisplay(selectedBeam) {
    console.log(`Updated display for selected beam: ${selectedBeam.designation}`);
    // This function maintains compatibility with existing code
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', initializeBeamCalculator);