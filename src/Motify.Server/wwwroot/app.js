const API_BASE = window.location.origin;
const API_KEY = 'tbx-dev-key-12345';

let currentResults = [];
let currentClientId = '';
let debugMode = false;

// Form submission
document.getElementById('uploadForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const formData = new FormData(e.target);
    currentClientId = formData.get('client_id');
    debugMode = document.getElementById('debugMode').checked;
    
    showLoader(true);
    
    try {
        const response = await fetch(`${API_BASE}/classify`, {
            method: 'POST',
            headers: {
                'Authorization': `ApiKey ${API_KEY}`
            },
            body: formData
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Classification failed');
        }
        
        const data = await response.json();
        currentResults = data.results;
        
        displayResults(data);
        showLoader(false);
        
    } catch (error) {
        alert(`Error: ${error.message}`);
        showLoader(false);
    }
});

function displayResults(data) {
    // Show results section
    document.getElementById('resultsSection').style.display = 'block';
    
    // Update stats
    const stats = document.getElementById('stats');
    stats.innerHTML = `
        <span>Total: ${data.count}</span>
        <span>New: ${data.new_accounts}</span>
        <span>Renumbered: ${data.renumbered_accounts}</span>
    `;
    
    // Populate table
    const tbody = document.getElementById('resultsBody');
    tbody.innerHTML = '';
    
    data.results.forEach((result, index) => {
        const row = createResultRow(result, index);
        tbody.appendChild(row);
    });
    
    // Scroll to results
    document.getElementById('resultsSection').scrollIntoView({ behavior: 'smooth' });
}

function createResultRow(result, index) {
    const tbody = document.getElementById('resultsBody');
    
    const tr = document.createElement('tr');
    if (result.needs_review) {
        tr.classList.add('needs-review');
    }
    
    // Account Code
    const tdCode = document.createElement('td');
    tdCode.textContent = result.account_code;
    tr.appendChild(tdCode);
    
    // Account Name
    const tdName = document.createElement('td');
    tdName.textContent = result.account_name;
    if (result.renumbered_from_name) {
        tdName.innerHTML += `<br><small style="color: #666;">From: ${result.renumbered_from_name}</small>`;
    }
    tr.appendChild(tdName);
    
    // Parent Code
    const tdParent = document.createElement('td');
    tdParent.textContent = result.parent_code || '-';
    tr.appendChild(tdParent);
    
    // Status
    const tdStatus = document.createElement('td');
    const statusClass = result.status === 'new' ? 'status-new' : 'status-renumbered';
    tdStatus.innerHTML = `<span class="status-badge ${statusClass}">${result.status}</span>`;
    tr.appendChild(tdStatus);
    
    // Category (editable)
    const tdCategory = document.createElement('td');
    const categoryInput = document.createElement('input');
    categoryInput.type = 'text';
    categoryInput.className = 'category-input';
    categoryInput.value = result.suggested_category || '';
    categoryInput.dataset.index = index;
    categoryInput.addEventListener('change', (e) => {
        currentResults[index].suggested_category = e.target.value;
    });
    tdCategory.appendChild(categoryInput);
    tr.appendChild(tdCategory);
    
    // Confidence
    const tdConfidence = document.createElement('td');
    tdConfidence.appendChild(createConfidenceBar(result.confidence));
    tr.appendChild(tdConfidence);
    
    // Needs Review (checkbox)
    const tdReview = document.createElement('td');
    const reviewCheckbox = document.createElement('input');
    reviewCheckbox.type = 'checkbox';
    reviewCheckbox.checked = result.needs_review;
    reviewCheckbox.dataset.index = index;
    reviewCheckbox.addEventListener('change', (e) => {
        currentResults[index].needs_review = e.target.checked;
        if (e.target.checked) {
            tr.classList.add('needs-review');
        } else {
            tr.classList.remove('needs-review');
        }
    });
    tdReview.appendChild(reviewCheckbox);
    tr.appendChild(tdReview);
    
    // Evidence
    const tdEvidence = document.createElement('td');
    const evidenceToggle = document.createElement('span');
    evidenceToggle.className = 'evidence-toggle';
    evidenceToggle.textContent = 'View';
    evidenceToggle.onclick = () => toggleEvidence(index);
    tdEvidence.appendChild(evidenceToggle);
    tr.appendChild(tdEvidence);
    
    // Evidence details row (hidden by default)
    const trEvidence = document.createElement('tr');
    trEvidence.id = `evidence-${index}`;
    trEvidence.style.display = 'none';
    const tdEvidenceDetails = document.createElement('td');
    tdEvidenceDetails.colSpan = 8;
    const evidenceDiv = document.createElement('div');
    evidenceDiv.className = 'evidence-details';
    evidenceDiv.textContent = formatEvidence(result.evidence);
    tdEvidenceDetails.appendChild(evidenceDiv);
    trEvidence.appendChild(tdEvidenceDetails);
    
    // Insert both rows
    tbody.appendChild(tr);
    tbody.appendChild(trEvidence);
    
    return tr;
}

function createConfidenceBar(confidence) {
    const container = document.createElement('div');
    container.className = 'confidence-bar';
    
    const value = document.createElement('span');
    value.className = 'confidence-value';
    value.textContent = `${confidence}%`;
    
    const meter = document.createElement('div');
    meter.className = 'confidence-meter';
    
    const fill = document.createElement('div');
    fill.className = 'confidence-fill';
    if (confidence >= 70) {
        fill.classList.add('confidence-high');
    } else if (confidence >= 40) {
        fill.classList.add('confidence-medium');
    } else {
        fill.classList.add('confidence-low');
    }
    fill.style.width = `${confidence}%`;
    
    meter.appendChild(fill);
    container.appendChild(value);
    container.appendChild(meter);
    
    return container;
}

function formatEvidence(evidenceJson) {
    try {
        const evidence = JSON.parse(evidenceJson);
        return JSON.stringify(evidence, null, 2);
    } catch {
        return evidenceJson;
    }
}

function toggleEvidence(index) {
    const evidenceRow = document.getElementById(`evidence-${index}`);
    if (evidenceRow.style.display === 'none') {
        evidenceRow.style.display = 'table-row';
    } else {
        evidenceRow.style.display = 'none';
    }
}

async function applyChanges() {
    if (!currentClientId || currentResults.length === 0) {
        alert('No results to apply');
        return;
    }
    
    try {
        // Update memory for each result
        for (const result of currentResults) {
            if (result.suggested_category && result.suggested_category !== 'Uncategorized') {
                const nameNorm = normalizeName(result.account_name);
                const parentNorm = result.parent_code ? normalizeName(result.parent_code) : null;
                
                const mapping = {
                    client_id: currentClientId,
                    name_norm: nameNorm,
                    parent_norm: parentNorm,
                    category: result.suggested_category,
                    source: 'ui',
                    updated_at: new Date().toISOString()
                };
                
                await fetch(`${API_BASE}/memory/update`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `ApiKey ${API_KEY}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(mapping)
                });
            }
        }
        
        alert('Changes applied successfully!');
    } catch (error) {
        alert(`Error applying changes: ${error.message}`);
    }
}

function normalizeName(name) {
    if (!name) return '';
    let normalized = name.toLowerCase();
    const charsToRemove = ['-', '_', '/', '(', ')', '.', ',', '"', ' '];
    charsToRemove.forEach(c => {
        normalized = normalized.replace(new RegExp('\\' + c, 'g'), '');
    });
    return normalized;
}

async function downloadCSV() {
    if (currentResults.length === 0) {
        alert('No results to download');
        return;
    }
    
    // Create CSV content
    const headers = ['account_code', 'account_name', 'parent_code', 'status', 'suggested_category', 
                     'confidence', 'needs_review', 'renumbered_from_code', 'renumbered_from_name'];
    
    let csv = headers.join(',') + '\n';
    
    currentResults.forEach(result => {
        const row = [
            result.account_code,
            `"${result.account_name}"`,
            result.parent_code || '',
            result.status,
            result.suggested_category || '',
            result.confidence,
            result.needs_review,
            result.renumbered_from_code || '',
            result.renumbered_from_name || ''
        ];
        csv += row.join(',') + '\n';
    });
    
    // Download
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `NewAccounts_${currentClientId}_${Date.now()}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
}

async function viewMemory() {
    if (!currentClientId) {
        alert('No client ID available');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE}/memory/${currentClientId}`, {
            headers: {
                'Authorization': `ApiKey ${API_KEY}`
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to load memory');
        }
        
        const data = await response.json();
        displayMemory(data.mappings);
        
    } catch (error) {
        alert(`Error loading memory: ${error.message}`);
    }
}

function displayMemory(mappings) {
    document.getElementById('memorySection').style.display = 'block';
    
    const tbody = document.getElementById('memoryBody');
    tbody.innerHTML = '';
    
    if (mappings.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="text-align: center;">No memory mappings found</td></tr>';
        return;
    }
    
    mappings.forEach(mapping => {
        const tr = document.createElement('tr');
        
        tr.innerHTML = `
            <td>${mapping.name_norm}</td>
            <td>${mapping.parent_norm || '-'}</td>
            <td>${mapping.category}</td>
            <td>${mapping.source}</td>
            <td>${new Date(mapping.updated_at).toLocaleString()}</td>
        `;
        
        tbody.appendChild(tr);
    });
    
    document.getElementById('memorySection').scrollIntoView({ behavior: 'smooth' });
}

function closeMemory() {
    document.getElementById('memorySection').style.display = 'none';
}

function showLoader(show) {
    const btnText = document.getElementById('btnText');
    const btnLoader = document.getElementById('btnLoader');
    const submitBtn = document.querySelector('.btn-primary');
    
    if (show) {
        btnText.style.display = 'none';
        btnLoader.style.display = 'inline-block';
        submitBtn.disabled = true;
    } else {
        btnText.style.display = 'inline';
        btnLoader.style.display = 'none';
        submitBtn.disabled = false;
    }
}

