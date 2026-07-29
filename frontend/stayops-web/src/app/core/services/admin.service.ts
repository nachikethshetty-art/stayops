import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/common.models';
import {
  AgentRateContract,
  CancellationPolicy,
  Company,
  CorporateRateContract,
  GstRule,
  RatePlan,
  RatePlanPrice,
  TravelAgent
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class RatePlanService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getByHotel(hotelId: string): Observable<RatePlan[]> {
    return this.http.get<RatePlan[]>(`${this.base}/hotels/${hotelId}/rate-plans`);
  }

  create(hotelId: string, request: Partial<RatePlan>): Observable<RatePlan> {
    return this.http.post<RatePlan>(`${this.base}/hotels/${hotelId}/rate-plans`, request);
  }

  update(hotelId: string, id: string, request: Partial<RatePlan>): Observable<RatePlan> {
    return this.http.put<RatePlan>(`${this.base}/hotels/${hotelId}/rate-plans/${id}`, request);
  }

  getPrices(hotelId: string, ratePlanId: string): Observable<RatePlanPrice[]> {
    return this.http.get<RatePlanPrice[]>(`${this.base}/hotels/${hotelId}/rate-plans/${ratePlanId}/prices`);
  }

  addPrice(hotelId: string, ratePlanId: string, request: Partial<RatePlanPrice>): Observable<RatePlanPrice> {
    return this.http.post<RatePlanPrice>(`${this.base}/hotels/${hotelId}/rate-plans/${ratePlanId}/prices`, request);
  }

  deletePrice(hotelId: string, ratePlanId: string, priceId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/hotels/${hotelId}/rate-plans/${ratePlanId}/prices/${priceId}`);
  }
}

@Injectable({ providedIn: 'root' })
export class CorporateService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  searchCompanies(page: number, pageSize: number, search?: string): Observable<PagedResult<Company>> {
    const params: Record<string, string> = { page: String(page), pageSize: String(pageSize) };
    if (search) params['search'] = search;
    return this.http.get<PagedResult<Company>>(`${this.base}/companies`, { params });
  }

  createCompany(request: Partial<Company>): Observable<Company> {
    return this.http.post<Company>(`${this.base}/companies`, request);
  }

  updateCompany(id: string, request: Partial<Company>): Observable<Company> {
    return this.http.put<Company>(`${this.base}/companies/${id}`, request);
  }

  getContracts(hotelId: string): Observable<CorporateRateContract[]> {
    return this.http.get<CorporateRateContract[]>(`${this.base}/hotels/${hotelId}/corporate-contracts`);
  }

  createContract(hotelId: string, request: Partial<CorporateRateContract> & { companyId: string; ratePlanId: string }): Observable<CorporateRateContract> {
    return this.http.post<CorporateRateContract>(`${this.base}/hotels/${hotelId}/corporate-contracts`, request);
  }

  updateContract(hotelId: string, contractId: string, request: Partial<CorporateRateContract>): Observable<CorporateRateContract> {
    return this.http.put<CorporateRateContract>(`${this.base}/hotels/${hotelId}/corporate-contracts/${contractId}`, request);
  }

  getTravelAgents(): Observable<TravelAgent[]> {
    return this.http.get<TravelAgent[]>(`${this.base}/travel-agents`);
  }

  createTravelAgent(request: Partial<TravelAgent>): Observable<TravelAgent> {
    return this.http.post<TravelAgent>(`${this.base}/travel-agents`, request);
  }

  getAgentContracts(hotelId: string): Observable<AgentRateContract[]> {
    return this.http.get<AgentRateContract[]>(`${this.base}/hotels/${hotelId}/agent-contracts`);
  }

  createAgentContract(hotelId: string, request: Partial<AgentRateContract> & { travelAgentId: string; ratePlanId: string }): Observable<AgentRateContract> {
    return this.http.post<AgentRateContract>(`${this.base}/hotels/${hotelId}/agent-contracts`, request);
  }
}

@Injectable({ providedIn: 'root' })
export class CancellationPolicyService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getByHotel(hotelId: string): Observable<CancellationPolicy[]> {
    return this.http.get<CancellationPolicy[]>(`${this.base}/hotels/${hotelId}/cancellation-policies`);
  }

  create(hotelId: string, name: string): Observable<CancellationPolicy> {
    return this.http.post<CancellationPolicy>(`${this.base}/hotels/${hotelId}/cancellation-policies`, { name });
  }

  update(hotelId: string, id: string, name: string, isActive: boolean): Observable<CancellationPolicy> {
    return this.http.put<CancellationPolicy>(`${this.base}/hotels/${hotelId}/cancellation-policies/${id}`, { name, isActive });
  }

  addRule(hotelId: string, policyId: string, request: Record<string, unknown>): Observable<CancellationPolicy> {
    return this.http.post<CancellationPolicy>(`${this.base}/hotels/${hotelId}/cancellation-policies/${policyId}/rules`, request);
  }

  deleteRule(hotelId: string, policyId: string, ruleId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/hotels/${hotelId}/cancellation-policies/${policyId}/rules/${ruleId}`);
  }
}

@Injectable({ providedIn: 'root' })
export class GstRuleService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getForHotel(hotelId: string): Observable<GstRule[]> {
    return this.http.get<GstRule[]>(`${this.base}/hotels/${hotelId}/gst-rules`);
  }

  create(hotelId: string, request: Record<string, unknown>): Observable<GstRule> {
    return this.http.post<GstRule>(`${this.base}/hotels/${hotelId}/gst-rules`, request);
  }

  update(hotelId: string, id: string, request: Record<string, unknown>): Observable<GstRule> {
    return this.http.put<GstRule>(`${this.base}/hotels/${hotelId}/gst-rules/${id}`, request);
  }
}
