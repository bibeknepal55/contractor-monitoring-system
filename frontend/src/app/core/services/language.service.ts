import { Injectable, signal } from '@angular/core';
import moment from 'moment-timezone';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly currentLang = signal<'en' | 'ne'>('en');

  private readonly translations: Record<string, Record<string, string>> = {
    en: {
      'CMS': 'CMS', 'Dashboard': 'Dashboard', 'Projects': 'Projects',
      'Contractors': 'Contractors', 'Business Modules': 'Business Modules',
      'Contract Financials': 'Contract Financials', 'Price Adjustments': 'Price Adjustments',
      'Performance Bonds': 'Performance Bonds', 'Advance Payment Guarantees': 'Advance Payment Guarantees',
      'Physical Progress': 'Physical Progress', 'Time Extensions': 'Time Extensions',
      'Delay Reasons': 'Delay Reasons', 'Raw Materials': 'Raw Materials',
      'Lab Tests': 'Lab Tests', 'Photo Monitoring': 'Photo Monitoring',
      'Subcontractors': 'Subcontractors', 'Responsible Officials': 'Responsible Officials',
      'Approval Workflow': 'Approval Workflow', 'Reports': 'Reports & Analytics',
      'User Management': 'User Management', 'Role Management': 'Role Management',
      'Main Menu': 'Main Menu', 'Logout': 'Logout', 'My Profile': 'My Profile',
      'Change Password': 'Change Password', 'Pending Approvals': 'Pending Approvals',
      'Search': 'Search', 'New Request': 'New Request', 'Submit': 'Submit',
      'Cancel': 'Cancel', 'Add': 'Add', 'Edit': 'Edit', 'Delete': 'Delete',
      'Save': 'Save', 'Active': 'Active', 'Inactive': 'Inactive',
      'Pending': 'Pending', 'Approved': 'Approved', 'Rejected': 'Rejected',
      'Expired': 'Expired', 'Expiring Soon': 'Expiring Soon', 'Completed': 'Completed',
      'Personal Info': 'Personal Info', 'Preferences': 'Preferences',
      'Security': 'Security', 'Sessions': 'Sessions', 'Activity Log': 'Activity Log',
      'Save Changes': 'Save Changes', 'Save Preferences': 'Save Preferences',
      'Regional Settings': 'Regional Settings', 'Notification Preferences': 'Notification Preferences',
      'Project': 'Project', 'Contractor': 'Contractor', 'Status': 'Status',
      'Priority': 'Priority', 'Budget': 'Budget', 'Location': 'Location',
      'Start Date': 'Start Date', 'End Date': 'End Date', 'Description': 'Description',
      'Actions': 'Actions', 'No records': 'No records found', 'Generate': 'Generate',
      'Export': 'Export', 'Approve': 'Approve', 'Reject': 'Reject',
      'Close': 'Close', 'Submit Request': 'Submit Request', 'Refresh': 'Refresh',
      'View': 'View', 'Record': 'Record', 'Module': 'Module', 'Level': 'Level',
      'Submitted By': 'Submitted By', 'Date': 'Date', 'Requested By': 'Requested By',
      'Comments': 'Comments', 'Record / Comments': 'Record / Comments',
    },
    ne: {
      'CMS': 'सीएमएस', 'Dashboard': 'ड्यासबोर्ड', 'Projects': 'परियोजनाहरू',
      'Contractors': 'ठेकेदारहरू', 'Business Modules': 'व्यवसाय मोड्युल',
      'Contract Financials': 'ठेक्का वित्त', 'Price Adjustments': 'मूल्य समायोजन',
      'Performance Bonds': 'कार्यसम्पादन बन्ड', 'Advance Payment Guarantees': 'अग्रिम भुक्तानी ग्यारेन्टी',
      'Physical Progress': 'भौतिक प्रगति', 'Time Extensions': 'समय विस्तार',
      'Delay Reasons': 'ढिलाइ कारण', 'Raw Materials': 'कच्चा पदार्थ',
      'Lab Tests': 'प्रयोगशाला परीक्षण', 'Photo Monitoring': 'फोटो निगरानी',
      'Subcontractors': 'उप-ठेकेदार', 'Responsible Officials': 'जिम्मेवार अधिकारी',
      'Approval Workflow': 'स्वीकृति प्रक्रिया', 'Reports': 'प्रतिवेदन',
      'User Management': 'प्रयोगकर्ता व्यवस्थापन', 'Role Management': 'भूमिका व्यवस्थापन',
      'Main Menu': 'मुख्य मेनु', 'Logout': 'लगआउट', 'My Profile': 'मेरो प्रोफाइल',
      'Change Password': 'पासवर्ड परिवर्तन', 'Pending Approvals': 'विचाराधीन स्वीकृति',
      'Search': 'खोज्नुहोस्', 'New Request': 'नयाँ अनुरोध', 'Submit': 'पेश गर्नुहोस्',
      'Cancel': 'रद्द गर्नुहोस्', 'Add': 'थप्नुहोस्', 'Edit': 'सम्पादन',
      'Delete': 'मेटाउनुहोस्', 'Save': 'सुरक्षित', 'Active': 'सक्रिय',
      'Inactive': 'निष्क्रिय', 'Pending': 'विचाराधीन', 'Approved': 'स्वीकृत',
      'Rejected': 'अस्वीकृत', 'Expired': 'म्याद सकियो',
      'Expiring Soon': 'म्याद सकिन लाग्यो', 'Completed': 'सम्पन्न',
      'Personal Info': 'व्यक्तिगत जानकारी', 'Preferences': 'प्राथमिकताहरू',
      'Security': 'सुरक्षा', 'Sessions': 'सत्रहरू', 'Activity Log': 'गतिविधि अभिलेख',
      'Save Changes': 'परिवर्तन सुरक्षित', 'Save Preferences': 'प्राथमिकता सुरक्षित',
      'Regional Settings': 'क्षेत्रीय सेटिङ', 'Notification Preferences': 'सूचना प्राथमिकता',
      'Project': 'परियोजना', 'Contractor': 'ठेकेदार', 'Status': 'स्थिति',
      'Priority': 'प्राथमिकता', 'Budget': 'बजेट', 'Location': 'स्थान',
      'Start Date': 'सुरु मिति', 'End Date': 'अन्त्य मिति', 'Description': 'विवरण',
      'Actions': 'कार्यहरू', 'No records': 'कुनै रेकर्ड छैन', 'Generate': 'उत्पादन',
      'Export': 'निर्यात', 'Approve': 'स्वीकृत', 'Reject': 'अस्वीकृत',
      'Close': 'बन्द', 'Submit Request': 'अनुरोध पेश गर्नुहोस्', 'Refresh': 'ताजा',
      'View': 'हेर्नुहोस्', 'Record': 'रेकर्ड', 'Module': 'मोड्युल',
      'Level': 'स्तर', 'Submitted By': 'पेश गर्ने', 'Date': 'मिति',
      'Requested By': 'अनुरोधकर्ता', 'Comments': 'टिप्पणी',
      'Record / Comments': 'रेकर्ड / टिप्पणी',
    }
  };

  constructor() {
    const saved = localStorage.getItem('language');
    if (saved === 'ne') { this.currentLang.set('ne'); moment.locale('ne'); }
    else { this.currentLang.set('en'); moment.locale('en'); }
  }

  t(key: string): string { return this.translations[this.currentLang()]?.[key] || key; }

  setLanguage(lang: 'en' | 'ne'): void {
    this.currentLang.set(lang);
    localStorage.setItem('language', lang);
    moment.locale(lang === 'ne' ? 'ne' : 'en');
  }

  getLang(): 'en' | 'ne' { return this.currentLang(); }
}