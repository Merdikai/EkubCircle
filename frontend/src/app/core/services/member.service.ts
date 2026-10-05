import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CircleMember, AddMemberRequest, JoinRequest, ReviewJoinRequestDto } from '../models';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);

  getCircleMembers(circleId: number): Observable<CircleMember[]> {
    return this.http.get<CircleMember[]>(`/api/circles/${circleId}/members`);
  }

  addMember(circleId: number, request: AddMemberRequest): Observable<CircleMember> {
    return this.http.post<CircleMember>(`/api/circles/${circleId}/members`, request);
  }

  getJoinRequests(circleId?: number): Observable<JoinRequest[]> {
    const url = circleId ? `/api/circles/${circleId}/join-requests` : '/api/join-requests';
    return this.http.get<JoinRequest[]>(url);
  }

  reviewJoinRequest(dto: ReviewJoinRequestDto): Observable<{ success: boolean; message: string }> {
    const action = dto.action.toLowerCase();
    return this.http.post<{ success: boolean; message: string }>(`/api/join-requests/${dto.requestId}/${action}`, {});
  }
}
