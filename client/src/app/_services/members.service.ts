import { HttpClient, HttpHeaders, HttpParams, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, pipe } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/Member';
import { User } from '../_models/User';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult } from './paginationHelper';
@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiURl
  members: Member[] = [];
  memberCahe = new Map();
  user: User;
  userParams: UserParams


  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    })
  }
  getUserParams() {
    return this.userParams;
  }
  setUserParams(params: UserParams) {
    this.userParams = params

  }
  resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }
  getMembers(userParams: UserParams) {
    // if (this.members.length > 0) {
    //   return of(this.members);
    // }
    var response = this.memberCahe.get(Object.values(userParams).join('-'))
    if (response) {
      return of(response)
    }
    let params = new HttpParams();
    if (userParams.pageNumber !== null && userParams.pageSize !== null) {
      params = params.append("pageNumber", userParams.pageNumber.toString());
      params = params.append("pageSize", userParams.pageSize.toString())
      params = params.append("minAge", userParams.minAge.toString())
      params = params.append("maxAge", userParams.maxAge.toString())
      params = params.append("gender", userParams.gender.toString())
      params = params.append('orderBy', userParams.orderBy)
    }
    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(map(response => {
      this.memberCahe.set(Object.values(userParams).join('-'), response);
      return response
    }))
  }


  getMember(username: string) {
    console.log(this.memberCahe)
    const member = [...this.memberCahe.values()].reduce((perv, curr) =>
      perv.concat(curr.result)
      , []).find((member: Member) => member.username === username)
    if (member != null) {
      return of(member)
    }
    return this.http.get<Member>(this.baseUrl + 'users/' + username)
  }
  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member)
        this.members[index] = member
      })
    )
  }
  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + `users/set-main-photo/${photoId}`, {})
  }
  deletePhoto(PhotoId: number) {
    return this.http.delete(this.baseUrl + `users/delete-photo/${PhotoId}`)
  }
  addLike(username: string) {
    return this.http.post(this.baseUrl + `likes/${username}`, {})
  }
  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = new HttpParams();
    params = params.append("pageNumber", pageNumber.toString())
    params = params.append("pageSize", pageSize.toString())
    params = params.append("predicate", predicate)
    return getPaginatedResult<Partial<Member[]>>(this.baseUrl + 'likes', params, this.http)
  }
}
